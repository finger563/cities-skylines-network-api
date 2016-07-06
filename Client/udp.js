const dgram = require('dgram');
const client = dgram.createSocket('udp4');

client.on('error', (err) => {
    console.log(`client error:\n${err.stack}`);
    client.close();
    process.exit(1);
});

client.on('message', (msg, rinfo) => {
    console.log(`client got: ${msg} from ${rinfo.address}:${rinfo.port}`);
    process.exit();
});

var message = new Buffer('/managers');

if (process.argv[2])
    message = process.argv[2];

client.send(message, 0, message.length, 11000, 'localhost', (err) => {
});