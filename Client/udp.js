const dgram = require('dgram');
const Q = require('q');
const client = dgram.createSocket('udp4');

var simFrame = 0;

var simFrameRecvFunc = function(msg) {
    var obj = JSON.parse(msg);
    simFrame = +obj;
    console.log(simFrame);
};

var send = function(msg, cb) {
    var deferred = Q.defer();

    client.on('message', (msg, rinfo) => {
	cb(msg);
	deferred.resolve();
    });

    client.on('error', (err) => {
	deferred.reject(err);
	client.close();
	process.exit(1);
    });

    console.log('Sending: '+msg);
    client.send(msg, 0, msg.length, 11000, 'localhost');

    return deferred.promise;
};

var nmp = {
    "useInstance": true,
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

var cmp = {
    "useInstance": true,
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

var newTest = {
    "get": [
	{
	    "name": "RoadBaseAI",
	    "type": "class",
	    "assembly": "Assembly-CSharp"
	},
	{
	    "name": "GetTrafficLightState",
	    "type": "method",
	    "parameters": [
		{
		    "name": "nodeId",
		    "type": "System.UInt32",
			"value": 154
		},
		{
		    "name": "segment",
		    "type": "NetSegment&",
		    "value": {
			"name": "NetManager",
			"type": "class",
			"assembly": "Assembly-CSharp",
			"then": {
			    "name": "instance",
			    "type": "member",
			    "then": {
				"name": "m_segments",
				"then": {
				    "name": "m_buffer",
				    "index": 0
				}
			    }
			}
		    }
		},
		{
		    "name": "simulation frame",
		    "type": "System.UInt32",
		    "value": {
			"name": "SimulationManager",
			"type": "class",
			"assembly": "Assembly-CSharp",
			"then": {
			    "name": "instance",
			    "type": "member",
			    "then": {
				"name": "m_currentFrameIndex"
			    }
			}
		    }
		},
		{
		    "name": "vehicle light state",
		    "type": "RoadBaseAI+TrafficLightState",
		    "value": {
		    }
		},
		{
		    "name": "pedestrian light state",
		    "type": "RoadBaseAI+TrafficLightState",
		    "value": {
		    }
		},
		{
		    "name": "vehicles",
		    "type": "System.Boolean",
		    "value": false
		},
		{
		    "name": "pedestrians",
		    "type": "System.Boolean",
		    "value": false
		}
	    ]
	}
    ]
};

newTest = {
    "get": [
	{
	    "name": "NetManager",
	    "type": "class",
	    "assembly": "Assembly-CSharp"
	},
	{
	    "name": "instance",
	    "type": "property"
	},
	{
	    "name": "m_segments",
	    "type": "member"
	},
	{
	    "name": "m_buffer",
	    "type": "member",
	    "index": 0
	}
    ]
};

var rbaip = {
    "useInstance": false,
    "parameters": [
	{
	    "name": "nodeID",
	    "type": "System.UInt16",
	},
	{
	    "name": "segmentData",
	    "type": "NetSegment&",
	    "assembly": "Assembly-CSharp"
	},
	{
	    "name": "frame",
	    "type": "System.UInt32",
	},
	{
	    "name": "vehicleLightState",
	    "type": "RoadBaseAI+TrafficLightState",
	    "assembly": "Assembly-CSharp"
	},
	{
	    "name": "pedestrianLightState",
	    "type": "RoadBaseAI+TrafficLightState",
	    "assembly": "Assembly-CSharp"
	},
	{
	    "name": "vehicles",
	    "type": "System.Boolean",
	    "value": false
	},
	{
	    "name": "pedestrians",
	    "type": "System.Boolean",
	    "value": false
	}
    ]
};

var smMessage = new Buffer(
    'Assembly-CSharp/SimulationManager/fields/m_currentFrameIndex'
);

var newTestMessage = new Buffer(
    JSON.stringify(newTest)
);

var finalRcvFunc = function(msg) {
    var obj = JSON.parse(msg);
    console.log(JSON.stringify(obj, null, 2));
    process.exit(0);
};

return send(newTestMessage, finalRcvFunc);
/*
return send(smMessage, simFrameRecvFunc)
    .then(() => {
	rbaip["parameters"][2]["value"] = simFrame;

	var cmMessage = new Buffer(
	    'Assembly-CSharp/CitizenManager/call/CreateCitizen?params=' +
		JSON.stringify(cmp)
	);

	var rbaiMessage = new Buffer(
	    'Assembly-CSharp/RoadBaseAI/call/SetTrafficLightState?params=' +
		JSON.stringify(rbaip)
	);

	var nmMessage = new Buffer(
	    'Assembly-CSharp/NetManager/call/GetSegment?params=' +
		JSON.stringify(nmp)
	);

	//var message = cmMessage;
	//var message = rbaiMessage;
	var message = nmMessage;

	if (process.argv[2])
	    message = process.argv[2];

	return send(message, finalRcvFunc);
    })
    .catch((err) => {
	console.log(err);
	process.exit(1);
    });
*/
