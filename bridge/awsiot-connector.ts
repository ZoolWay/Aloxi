import winston = require('winston');
import { readFileSync } from 'fs';
import { mqtt, io, iot } from 'aws-crt';
import { TextDecoder } from 'util';
let os = require('os');
import { NetworkInterfaceInfo } from 'os';

export interface AwsIotConfiguration {
    keyPath: string,
    certPath: string,
    caPath: string,
    clientId: string,
    topic: string,
    endpoint: string
}

function logPad(message: string): string {
    if ((message === undefined) || (message === null)) return message;
    return message.split("\n").join("\n                              ");
}

export class AwsIotConnector {
    private log: winston.Logger;
    private configFile: string;
    private config: AwsIotConfiguration;
    private conn: mqtt.MqttClientConnection;
    private onPublish: (payload: string) => void;

    public constructor(log: winston.Logger, configFile: string, onPublish: (payload: string) => void) {
        this.log = log;
        this.configFile = configFile;
        this.onPublish = onPublish;
    }

    public async start(): Promise<void> {
        this.config = this.readConfiguration(this.configFile);
        this.conn = this.createMqttClientConnection();
        await this.conn.connect();
        await this.subscribe();
        await this.publishBridge();
    }

    public stop(): Promise<void> {
        return this.conn.disconnect();
    }

    public publish(payload: mqtt.Payload): Promise<void> {
        const log = this.log;
        const configTopic = this.config.topic;
        return this.conn.publish(this.config.topic, payload, mqtt.QoS.AtLeastOnce)
            .then((v: mqtt.MqttRequest) => {
                log.debug(`Published to topic '${configTopic}', packet id ${v.packet_id}`);
            });
    }

    private subscribe(): Promise<void> {
        this.log.debug('Subscribing...');
        const log = this.log;
        const decoder = new TextDecoder('utf8');
        const configTopic = this.config.topic;
        const notifyOnPublish = this.onPublish;
        const onPublishCallback = async (topic: string, payload: ArrayBuffer) => {
            const json = decoder.decode(payload);
            log.debug(`Message received on topic ${topic}`);
            log.debug(logPad(json));
            notifyOnPublish(json);
        };
        return this.conn.subscribe(this.config.topic, mqtt.QoS.AtLeastOnce, onPublishCallback)
            .then((v: mqtt.MqttSubscribeRequest) => {
                log.debug(`Subscribed to topic '${configTopic}'`);
            });
    }

    private publishBridge(): Promise<void> {
        this.log.debug('Publishing bridge...');
        const log = this.log;
        const configTopic = this.config.topic;
        const payload = {
            message: `Hi, I am an Aloxi Bridge!`,
            timestamp: new Date().toISOString(),
            networkConnections: this.getIpAddresses()
        };
        return this.conn.publish(this.config.topic, payload, mqtt.QoS.AtLeastOnce)
            .then((v: mqtt.MqttRequest) => {
                log.debug(`Published bridge to topic '${configTopic}', packet id ${v.packet_id}`);
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
                topic: 'aloxi-bridge',
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