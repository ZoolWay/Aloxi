import winston = require('winston');
import { BridgeServer } from './bridge-server';


const log = winston.createLogger({
    level: 'debug', // on Pi set higher!!
    format: winston.format.combine(
        winston.format.timestamp({
            format: 'YYYY-MM-DD HH:mm:ss'
        }),
        winston.format.printf((info) => { return `${info.timestamp} ${info.level.toUpperCase()} [${info.module}] ${info.message}` }),
    ),
    transports: [
        new winston.transports.File({ filename: 'error.log', level: 'error' }),
        new winston.transports.Console()
    ]
});


log.debug('Launching', { 'module': 'Main' });
BridgeServer.launch(log);
