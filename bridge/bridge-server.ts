import { LoxoneConnector, LoxoneShadow } from './loxone-connector';
import express = require('express');
import winston = require('winston');
import { Server } from 'http';
import { ParamsDictionary, Query } from 'express-serve-static-core';
import { AwsIotConnector, AloxiMessage, AloxiOperation } from './awsiot-connector';


export class BridgeServer {
    private log: winston.Logger;
    private isStarted: boolean;
    private server: Server;
    private app: express.Application;
    private iotConnector: AwsIotConnector;
    private loxoneConnector: LoxoneConnector;

    public static launch(log: winston.Logger): BridgeServer {
        let bs = new BridgeServer();
        bs.log = log.child({ module: 'BridgeServer' });
        bs.start();
        return bs;
    }

    private constructor() {
        this.isStarted = false;
    }

    private async start(): Promise<void> {
        if (this.isStarted) {
            this.log.error('Already running!');
            return;
        }

        try {
            const port = 3000;

            this.log.debug('Preparing Express WebServer');
            this.app = express();
            this.server = this.app.listen(port, () => {
                this.log.info(`Aloxi Bridge listening on port ${port}`);
            });
            this.app.get('/stop', (req, resp) => this.handleWebStopRequest(req, resp));
            this.app.get('/status', (req, resp) => this.handleWebStatusRequest(req, resp));

            this.log.debug('Connecting to AWS IoT');
            const self = this;
            this.iotConnector = new AwsIotConnector(this.log, './config/aws-iot.json', (payload) => self.handleMessage(payload));
            await this.iotConnector.start();

            this.log.debug('Setting up Loxone connection');
            this.loxoneConnector = new LoxoneConnector(this.log, './config/loxone.json', (model) => self.handleModelUpdate(model));

            this.log.info('Aloxi Bridge server started');

            this.isStarted = true;
        } catch (err) {
            this.log.error('Failed to start Aloxi Bridge server: ' + err);
            this.isStarted = false;
        }
    }

    private stop(): void {
        if (!this.isStarted) {
            this.log.error('Cannot stop, not running!');
            return;
        }

        try {
            this.log.info('Closing server');
            this.server.close();
        } catch (err) {
            this.log.error('Failed to close server: ' + err);
        }

        try {
            this.iotConnector.stop();
        } catch (err) {
            this.log.error('Failed to stop AWS IoT Connector: ' + err);
        }

        this.isStarted = false;
    }

    private sendMessage(message: AloxiMessage): void {
        this.log.debug(`(cloud) <-- sending message '${message.operation}'`);
        this.iotConnector.publish(message);
    }

    private handleMessage(message: AloxiMessage): void {
        this.log.debug(`(cloud) --> message received '${message.operation}'`);
        switch (message.operation) {
            case 'echo':
                this.performOperationEcho(message.data);
                break;
        }
    }

    private handleModelUpdate(model: LoxoneShadow): void {
        this.log.debug('(lox) --> new model');
    }

    private handleWebStatusRequest(req: express.Request<ParamsDictionary, any, any, Query>, resp: express.Response<any>): void {
        this.log.debug('(web) --> GET /status');
        let statusObject = {
            isStarted: this.isStarted,
            loxone: (this.loxoneConnector ? this.loxoneConnector.getState() : 'not ready'),
            cloud: 'not implemented yet',
            alexa: 'not implemented yet',
            timestamp: new Date().toISOString()
        };
        resp.send(statusObject);
    }

    private handleWebStopRequest(req: express.Request<ParamsDictionary, any, any, Query>, resp: express.Response<any>): void {
        this.log.warn('User requested to stop!');
        resp.send('Trying to stop Aloxi Bridge');
        this.stop();
    }

    private createMessage(operation: AloxiOperation, data: any): AloxiMessage {
        return { 'type': 'aloxiComm', 'operation': operation, 'data': data };
    }

    private performOperationEcho(data: any): void {
        this.sendMessage(this.createMessage('echoResponse', data));
    }
}
