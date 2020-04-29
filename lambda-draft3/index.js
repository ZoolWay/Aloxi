const awsIot = require('aws-iot-device-sdk');
const topic = 'aloxi-bridge';
const endpoint = 'a1fk2tqo498isz-ats.iot.eu-west-1.amazonaws.com';

const device = awsIot.jobs({ host: endpoint, protocol: 'wss' });

device.on('connect', function () {
    console.log('connected. subscribing to topic...');
    device.subscribe(topic);
    console.log('subscribed to topic ' + topic);
});

var subscriber = null;
var lastEchoRequesst = null;

device.on('message', function (topic, payload) {
    console.log('incoming message on topic ' + topic + ': ' + JSON.stringify(payload));
    if ((payload.type === undefined) || (payload.type !== 'aloxiComm')) return;
    if (payload.operation === undefined) return;
    console.log('operation: ' + payload.operation);

    if (payload.operation === 'echoResponse') {
        if (subscriber) {
            console.log('calling subscriber');
            subscriber(topic, payload.data);
        } else {
            console.log("no subscriber");
        }
        return;
    }
    console.log("Not relevant operation: " + payload.operation);
});

exports.handler = async function (event, context) {
    console.log("EVENT: \n" + JSON.stringify(event, null, 2));

    lastEchoRequesst = "from-lambda" + new Date().toISOString();
    var deviceRequest = { "type": "aloxiComm", "operation": "echo", "data": lastEchoRequesst };

    const promise = new Promise(function (resolve, reject) {
        subscriber = function (topic, payload) {
            console.log('subscriber called, resolving promise...');
            if (payload == lastEchoRequesst) {
                console.log('echo MATCH');
            } else {
                console.log('echo FAIL');
            }
            resolve("SUCCESS " + payload);
        };
    });

    await device.publish(topic, JSON.stringify(deviceRequest, null, ''), { qos: 0 }, function (err, data) {
        //console.log("publishing message to device", err, data);
    });

    return promise;
};
