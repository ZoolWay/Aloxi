import winston = require('winston');
import { BridgeServer } from './bridge-server';


const log = winston.createLogger({
    level: 'debug', // on Pi set higher!!
    format: winston.format.combine(
        winston.format.colorize(),
        winston.format.timestamp({
            format: 'YYYY-MM-DD HH:mm:ss'
        }),
        winston.format.padLevels(),
        winston.format.printf((info) => { return `${info.timestamp} ${info.level.toUpperCase()}: ${info.message}` }),
    ),
    transports: [
        new winston.transports.File({ filename: 'error.log', level: 'error' }),
        new winston.transports.Console()
    ]
});


BridgeServer.launch(log);
