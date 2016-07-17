const dgram = require('dgram');
const client = dgram.createSocket('udp4');

client.on('error', (err) => {
    console.log(`client error:\n${err.stack}`);
    client.close();
    process.exit(1);
});

client.on('message', (msg, rinfo) => {
    var obj = JSON.parse(msg);
    console.log(JSON.stringify(obj, null, 2));
    process.exit();
});

var citizenManagerParams = {
    "parameters": [
	{
	    "name": "citizen",
	    "type": "System.UInt32&",
	    "value": "test string"
	},
	{
	    "name": "age",
	    "type": "System.Int32",
	    "value": 154
	},
	{
	    "name": "family",
	    "type": "System.Int32",
	    "value": 154
	},
	{
	    "name": "r",
	    "type": "ColossalFramework.Math.Randomizer&",
	    "value": 154
	}
    ]
};

var roadBaseAIParams = {
    "parameters": [
	{
	    "name": "nodeID",
	    "type": "System.UInt16",
	},
	{
	    "name": "segmentData",
	    "type": "NetSegment&",
	},
	{
	    "name": "frame",
	    "type": "System.UInt32",
	},
	{
	    "name": "vehicleLightState",
	    "type": "RoadBaseAI+TrafficLightState",
	},
	{
	    "name": "pedestrianLightState",
	    "type": "RoadBaseAI+TrafficLightState",
	},
	{
	    "name": "vehicles",
	    "type": "System.Boolean",
	},
	{
	    "name": "pedestrians",
	    "type": "System.Boolean",
	}
    ]
};

var cmMessage = new Buffer(
    'Assembly-CSharp/CitizenManager/call/CreateCitizen?params=' +
	JSON.stringify(citizenManagerParams)
);

var rbaiMessage = new Buffer(
    'Assembly-CSharp/RoadBaseAI/call/SetTrafficLightState?params=' +
	JSON.stringify(roadBaseAIParams)
);

var message = rbaiMessage;

if (process.argv[2])
    message = process.argv[2];

client.send(message, 0, message.length, 11000, 'localhost', (err) => {
});
