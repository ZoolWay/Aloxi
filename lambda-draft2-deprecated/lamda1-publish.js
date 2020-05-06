const awsIot = require('aws-sdk');



exports.handler = async (event) => {
    console.log('Start');
    try {
        let config = {
            endpoint: 'a1fk2tqo498isz-ats.iot.eu-west-1.amazonaws.com'
        };
        let iotdata = new awsIot.IotData(config);
        let params = {
            topic: 'aloxi-bridge',
            payload: { "type": "aloxiComm", "operation": "echo", "data": "from-lambda" + new Date().toISOString() },
            qos: 0
        };
        iotdata.subscribe()
        await iotdata.publish(params, function (err, data) {
            if (err) {
                console.log("ERROR:" + JSON.stringify(err));
            } else {
                console.log("Success");
            }
        }).promise();
    } catch (err) {
        console.log("ERROR(catch): " + err);
    }


    // TODO implement
    const response = {
        statusCode: 200,
        body: JSON.stringify('Hello from Lambda!'),
    };
    console.log('End');
    return response;
};
