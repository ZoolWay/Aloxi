// code samples for aws-iot-device-sdk-js-v2, issue #38
// change package.json main from index.js to minicode.js
// https://github.com/aws/aws-iot-device-sdk-js-v2/issues/38
// gist: https://gist.github.com/ZoolWay/3acda0ca48ecd54e57f531d1d7914266

import { mqtt, io, iot } from 'aws-crt';
import express = require('express');

const config = {
    "keyPath": "config/abcd-private.pem.key",
    "certPath": "config/abcd-certificate.pem.crt",
    "caPath": "config/AmazonRootCA3.pem",
    "clientId": "my-client-id",
    "topic": "my-topic",
    "endpoint": "xyz-ats.iot.eu-west-1.amazonaws.com"
};

function createMqttClientConnection(): mqtt.MqttClientConnection {
    const clientBootstrap = new io.ClientBootstrap();
    const configBuilder = iot.AwsIotMqttConnectionConfigBuilder.new_mtls_builder_from_path(config.certPath, config.keyPath);
    configBuilder.with_certificate_authority_from_path(config.caPath);
    configBuilder.with_clean_session(true);
    configBuilder.with_client_id(config.clientId);
    configBuilder.with_endpoint(config.endpoint);
    const cb = configBuilder.build();
    const client = new mqtt.MqttClient(clientBootstrap);
    const connection = client.new_connection(cb);
    return connection;
}

function subscribe(cn: mqtt.MqttClientConnection): Promise<void> {
    const decoder = new TextDecoder('utf8');
    const configTopic = config.topic;
    const onPublishCallback = async (topic: string, payload: ArrayBuffer) => {
        const json = decoder.decode(payload);
        console.debug(`Message received on topic ${topic}: ${json}`);
    };
    return cn.subscribe(config.topic, mqtt.QoS.AtLeastOnce, onPublishCallback)
        .then((v: mqtt.MqttSubscribeRequest) => {
            console.debug(`Subscribed to topic '${configTopic}'`);
        });
}

async function scenarioAandB(): Promise<void> {
    console.debug('Scenario A/B');
    try {
        const cn = createMqttClientConnection();
        await cn.connect();
        await subscribe(cn);
        //  now we get messages and sometimes ping-timeouts
    } catch (err) {
        console.error("Catched: " + err);
    }
}

async function scenarioC(): Promise<void> {
    console.debug('Scenario C');
    try {
        config.keyPath = config.keyPath + 'foo';
        const cn = createMqttClientConnection();
        await cn.connect();
        await subscribe(cn); // not reached
    } catch (err) {
        console.error("Catched: " + err);
    }
}

async function scenarioD(): Promise<void> {
    console.debug('Scenario D');
    try {
        config.keyPath = config.keyPath.replace('private', 'public');
        const cn = createMqttClientConnection();
        await cn.connect();
        await subscribe(cn); // not reached, crash
    } catch (err) {
        console.error("Catched: " + err)
    }
}


const app = express();
const server = app.listen(3000, () => {
    console.debug('Express runs and keeps the app from closing, providing info via HTTP');
});
app.get('/', (req, resp) => resp.send("I'm still here!"));

scenarioAandB();
//scenarioC();
//scenarioD();
