import { AppConfiguration } from './lib/AppConfiguration';
import { mqtt, io, iot } from 'aws-crt';
import { TextDecoder } from 'util';
import { readFileSync } from 'fs';

async function execute_session(connection: mqtt.MqttClientConnection) {
    const topic = 'aloxi-bridge';

    return new Promise(async (resolve, reject) => {
        try {
            const decoder = new TextDecoder('utf8');
            const on_publish = async (topic: string, payload: ArrayBuffer) => {
                const json = decoder.decode(payload);
                console.log(`Publish received on topic ${topic}`);
                console.log(json);
                /*if (message.sequence == argv.count) {
                    resolve();
                }*/
            }

            await connection.subscribe(topic, mqtt.QoS.AtLeastOnce, on_publish);

            const heloMessage = {
                message: "Hi, I am the bridge!"
            };
            connection.publish(topic, JSON.stringify(heloMessage), mqtt.QoS.AtLeastOnce);
        }
        catch (error) {
            reject(error);
        }
    });
}

function createMqttClientConnection(appConfig: AppConfiguration): mqtt.MqttClientConnection {
    const clientBootstrap = new io.ClientBootstrap();
    const configBuilder = iot.AwsIotMqttConnectionConfigBuilder.new_mtls_builder_from_path(appConfig.certPath, appConfig.keyPath);
    configBuilder.with_certificate_authority_from_path(appConfig.caPath);
    configBuilder.with_clean_session(true);
    configBuilder.with_client_id(appConfig.clientId);
    configBuilder.with_endpoint(appConfig.endpoint);
    const config = configBuilder.build();
    const client = new mqtt.MqttClient(clientBootstrap);
    const connection = client.new_connection(config);
    return connection;
}

async function main(): Promise<void> {
    const appConfig: AppConfiguration = JSON.parse(readFileSync('./config/aws-iot.json', 'UTF-8'));

    const timer = setTimeout(() => { }, 10 * 60 * 1000);
    const connection = createMqttClientConnection(appConfig);
    await connection.connect();
    await execute_session(connection);

    // Allow node to die if the promise above resolved
    clearTimeout(timer);
}

main();