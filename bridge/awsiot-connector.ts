import winston = require('winston');
import { readFileSync } from 'fs';
import { mqtt, io, iot } from 'aws-crt';
import { TextDecoder } from 'util';
let os = require('os');
import { NetworkInterfaceInfo } from 'os';

export type OnPublishHandler = (message: AloxiMessage) => void;
export type AloxiOperation = 'bridgeAnnouncement' | 'echo' | 'echoResponse' | 'pipeAlexaRequest' | 'pipeAlexaResponse';
export type ResponseTopic = string | undefined;
export interface AloxiMessage {
    type: 'aloxiComm';
    operation: AloxiOperation;
    responseTopic: ResponseTopic;
    data: any;
}

export interface AwsIotConfiguration {
    keyPath: string,
    certPath: string,
    caPath: string,
    clientId: string,
    topicReceive: string,
    announceTopics: string[],
    endpoint: string
}

export class AwsIotConnector {
    private log: winston.Logger;
    private configFile: string;
    private config: AwsIotConfiguration;
    private conn: mqtt.MqttClientConnection;
    private onPublish: OnPublishHandler;

    public constructor(log: winston.Logger, configFile: string, onPublish: OnPublishHandler) {
        this.log = log.child({ module: 'AwsIotConnector' });
        this.configFile = configFile;
        this.onPublish = onPublish;
    }

    public async start(): Promise<void> {
        try {
            this.config = this.readConfiguration(this.configFile);
            this.conn = this.createMqttClientConnection();
            await this.conn.connect();
            await this.subscribe();
            await this.publishBridge();
        } catch (err) {
            this.log.error("Starting AWS IoT connection failed: ", err);
        }
    }

    public stop(): Promise<void> {
        return this.conn.disconnect();
    }

    /**
     * Publishes a message to the given topic and returns its packet-id.
     * @param topic 
     * @param message 
     */
    public publish(topic: string, message: AloxiMessage): Promise<number> {
        if (message.type === undefined) message.type = 'aloxiComm';
        if (message.type !== 'aloxiComm') throw new Error('Invalid aloxi message type');
        if (message.operation === undefined) throw new Error('Aloxi message misses operation');
        return this.publishInternal(topic, message).then((req: mqtt.MqttRequest) => {
            if (req.packet_id === undefined) return -1;
            return req.packet_id;
        });
    }

    private publishInternal(topic: string, payload: mqtt.Payload): Promise<mqtt.MqttRequest> {
        const log = this.log;
        const destTopic = topic;
        return this.conn.publish(topic, payload, mqtt.QoS.AtLeastOnce)
            .then((req: mqtt.MqttRequest) => {
                log.debug(`Published to topic '${destTopic}', packet id ${req.packet_id}`);
                return req;
            });
    }

    private subscribe(): Promise<void> {
        this.log.debug('Subscribing...');
        const log = this.log;
        const decoder = new TextDecoder('utf8');
        const configTopic = this.config.topicReceive;
        const notifyOnPublish = this.onPublish;
        const onPublishCallback = async (topic: string, payload: ArrayBuffer) => {
            try {
                const payloadString = decoder.decode(payload);
                const message: AloxiMessage = JSON.parse(payloadString);
                let discard: boolean = ((message.type === undefined) || (message.type !== 'aloxiComm') || (message.operation === undefined));
                if (discard) {
                    log.debug('Discarding non-aloxi message');
                    return;
                }
                log.debug(`Received Aloxi message for operation ${message.operation}`);
                notifyOnPublish(message);
            } catch (err) {
                log.error('Failed to handle a received message: ' + err);
            }
        };
        return this.conn.subscribe(configTopic, mqtt.QoS.AtLeastOnce, onPublishCallback)
            .then((v: mqtt.MqttSubscribeRequest) => {
                log.debug(`Subscribed to topic '${configTopic}'`);
            });
    }

    private publishBridge(): Promise<void> {
        this.log.debug('Publishing bridge...');
        let announcePromises = new Array<Promise<mqtt.MqttRequest>>();
        const log = this.log;
        const announceToTopics = this.config.announceTopics;
        for (let announceToTopic of announceToTopics) {
            const message: AloxiMessage = {
                type: "aloxiComm",
                operation: "bridgeAnnouncement",
                responseTopic: this.config.topicReceive,
                data: {
                    message: `Hi, I am an Aloxi Bridge!`,
                    timestamp: new Date().toISOString(),
                    networkConnections: this.getIpAddresses()
                }
            }
            let p = this.publishInternal(announceToTopic, message);
            announcePromises.push(p);
        }
        return Promise.all(announcePromises).then((requests: mqtt.MqttRequest[]) => {
            log.debug(`Published bridge to ${requests.length} configured topics`);
        });
    }

    private readConfiguration(configFile: string): AwsIotConfiguration {
        let config: AwsIotConfiguration;
        try {
            config = JSON.parse(readFileSync(configFile, 'UTF-8'));
            this.log.debug(`Read AWS IoT configuration, endpoint: ${config.endpoint}`);
        } catch (err) {
            this.log.error('Failed to read AWS configuration: ' + err);
            config = {
                keyPath: '',
                certPath: '',
                caPath: '',
                clientId: 'aloxi-bridge-client',
                topicReceive: 'aloxi:to-bridge',
                announceTopics: ['aloxi:alexa-response', 'aloxi:to-bridge'],
                endpoint: ''
            };
        }
        return config;
    }

    private getIpAddresses(): string[] {
        let ifList: string[] = [];
        let ifaces = os.networkInterfaces();

        Object.keys(ifaces).forEach(function (ifname: string) {
            let alias = 0;

            ifaces[ifname].forEach(function (iface: NetworkInterfaceInfo) {
                if ('IPv4' !== iface.family || iface.internal !== false) {
                    // skip over internal (i.e. 127.0.0.1) and non-ipv4 addresses
                    return;
                }
                if (alias >= 1) {
                    // this single interface has multiple ipv4 addresses
                    ifList.push(`${ifname}:${alias} ${iface.address}`);
                } else {
                    // this interface has only one ipv4 adress
                    ifList.push(`${ifname} ${iface.address}`);
                }
                ++alias;
            });
        });

        return ifList;
    }

    private createMqttClientConnection(): mqtt.MqttClientConnection {
        const clientBootstrap = new io.ClientBootstrap();
        const configBuilder = iot.AwsIotMqttConnectionConfigBuilder.new_mtls_builder_from_path(this.config.certPath, this.config.keyPath);
        configBuilder.with_certificate_authority_from_path(this.config.caPath);
        configBuilder.with_clean_session(true);
        configBuilder.with_client_id(this.config.clientId);
        configBuilder.with_endpoint(this.config.endpoint);
        const config = configBuilder.build();
        const client = new mqtt.MqttClient(clientBootstrap);
        const connection = client.new_connection(config);
        return connection;
    }
}