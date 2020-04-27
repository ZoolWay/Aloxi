import winston = require('winston');
import { readFileSync } from 'fs';
import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';

interface LoxoneConfiguration {
    miniserver: string;
    username: string;
    password: string;
}

export type OnModelUpdateHandler = (model: any) => void;

export interface RoomDefinition {
    name: string;
    uuid: string;
}

export interface MiniserverShadow {
    name: string;
    project: string;
    localUrl: string;
}

export interface LoxoneShadow {
    miniserver: MiniserverShadow;
    retrievedAs: string;
    retrievedAsAdmin: boolean;
    retrievedAt: Date;
}

export interface ConnectorState {
    miniserverName: string;
    hasModel: boolean;
    isAdmin: boolean;
    modelCreated: Date | null;
}

export class LoxoneConnector {
    private log: winston.Logger;
    private configFile: string;
    private config: LoxoneConfiguration;
    private shadow: LoxoneShadow;
    private client: AxiosInstance;
    private onModelUpdate: OnModelUpdateHandler;

    public constructor(log: winston.Logger, configFile: string, onModelUpdate: OnModelUpdateHandler) {
        this.log = log.child({ module: 'LoxoneConnector' });
        this.configFile = configFile;
        this.onModelUpdate = onModelUpdate;
        this.init();
    }

    public init(): void {
        this.log.debug('Initializing LoxoneConnector');
        this.config = this.readConfiguration(this.configFile);
        let axiosConfig: AxiosRequestConfig = {
            withCredentials: true,
            auth: {
                username: this.config.username,
                password: this.config.password
            },
            timeout: 3000,
            baseURL: `http://${this.config.miniserver}/`
        };
        this.client = axios.create(axiosConfig);
        const log = this.log;
        this.buildModel().then((modelWasCreated: boolean) => {
            if (modelWasCreated) {
                log.info('Initial model build completed');
            } else {
                log.error('No initial model available!');
            }
        });
    }

    public async buildModel(): Promise<boolean> {
        try {
            this.log.debug('Retrieving structure from Loxone');
            const resp: AxiosResponse = await this.client.get('data/LoxAPP3.json');
            if (resp.status != 200) {
                this.log.error('Failed to get structure from Loxone! Keeping model. Response: ', resp);
                return false;
            }
            const struct = resp.data;

            let newModel: LoxoneShadow = {
                miniserver: {
                    name: struct.msInfo.msName,
                    project: struct.msInfo.projectName,
                    localUrl: struct.msInfo.localUrl
                },
                retrievedAs: struct.msInfo.currentUser.name,
                retrievedAsAdmin: struct.msInfo.currentUser.isAdmin,
                retrievedAt: new Date()
            };

            this.shadow = newModel;
            this.raiseOnModelUpdate(newModel);
            return true;
        } catch (err) {
            this.log.error('Failed to build model (keeping current): ', err);
            return false;
        }
    }

    public getState(): ConnectorState {
        if (!this.hasModel()) {
            return {
                miniserverName: 'unknown',
                hasModel: false,
                isAdmin: false,
                modelCreated: null
            };
        }
        const currentState: ConnectorState = {
            miniserverName: this.shadow.miniserver.name,
            hasModel: true,
            isAdmin: this.shadow.retrievedAsAdmin,
            modelCreated: this.shadow.retrievedAt
        };
        return currentState;
    }

    private async raiseOnModelUpdate(model: LoxoneShadow): Promise<void> {
        if (!(this.onModelUpdate)) return;
        try {
            this.onModelUpdate(model);
        } catch (err) {
            this.log.error('Failed to raise OnModelUpdate: ', err);
        }
    }

    private hasModel(): boolean {
        if (this.shadow === undefined || this.shadow === null) return false;
        return true;
    }

    private readConfiguration(configFile: string): LoxoneConfiguration {
        let config: LoxoneConfiguration;
        try {
            config = JSON.parse(readFileSync(configFile, 'UTF-8'));
            this.log.debug(`Read Loxone configuration, miniserver: ${config.miniserver}`);
        } catch (err) {
            this.log.error('Failed to read Loxone configuration: ' + err);
            config = {
                miniserver: 'localhost',
                username: '',
                password: ''
            };
        }
        return config;
    }
}