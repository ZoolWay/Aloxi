import express = require('express');
import winston = require('winston');
import { Server } from 'http';
import { ParamsDictionary, Query } from 'express-serve-static-core';
import { AwsIotConnector } from './awsiot-connector';


export class BridgeServer {
    private log: winston.Logger;
    private isStarted: boolean;
    private server: Server;
    private app: express.Application;
    private iotConnector: AwsIotConnector;

    public static launch(log: winston.Logger): BridgeServer {
        let bs = new BridgeServer();
        bs.log = log;
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
            this.app.get('/stop', (req, resp) => this.handleStop(req, resp));
            this.app.get('/status', (req, resp) => this.handleStatus(req, resp));

            this.iotConnector = new AwsIotConnector(this.log, './config/aws-iot.json');
            await this.iotConnector.start();

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

    private handleStatus(req: express.Request<ParamsDictionary, any, any, Query>, resp: express.Response<any>): void {
        let statusObject = {
            isStarted: this.isStarted,
            timestamp: new Date().toISOString()
        };
        resp.send(statusObject);
    }

    private handleStop(req: express.Request<ParamsDictionary, any, any, Query>, resp: express.Response<any>): void {
        this.log.warn('User requested to stop!');
        resp.send('Trying to stop Aloxi Bridge');
        this.stop();
    }
}