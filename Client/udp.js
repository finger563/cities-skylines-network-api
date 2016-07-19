const dgram = require('dgram');
const Q = require('q');
const client = dgram.createSocket('udp4');

var simFrame = 0;

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

var frameTest = {
    'Method': 'GET',
    'Object': {
	'Name': 'm_currentFrameIndex',
	'Type': 'MEMBER',
	'IsStatic': false,
	'Dependency': {
	    'Name': 'instance',
	    'IsStatic': true,
	    'Type': 'MEMBER',
	    'Dependency': {
		'Assembly': 'Assembly-CSharp',
		'Name': 'SimulationManager',
		'Type': 'CLASS'
	    }
	}
    }
};

var bufferTest = {
};

var methodTest = {
};

var intTest = {
    'Method': 'GET',
    'Object': {
	'Type': 'PARAMETER',
	'Value': 154,
	'ValueType': 'System.UInt32',
    }
};

var enumTest = {
    'Method': 'GET',
    'Object': {
	'Type': 'PARAMETER',
	'Value': 'GreenToRed',
	'ValueType': 'RoadBaseAI+TrafficLightState',
	'Assembly': 'Assembly-CSharp'
    }
};

var test = frameTest;

if (process.argv[2]) {
    if (process.argv[2] == "buffer")
	test = bufferTest;
    else if (process.argv[2] == "method")
	test = methodTest;
    else if (process.argv[2] == "int")
	test = intTest;
    else if (process.argv[2] == "enum")
	test = enumTest;
}

var testMessage = new Buffer(
    JSON.stringify(test)
);

var finalRcvFunc = function(msg) {
    var obj = JSON.parse(msg);
    console.log(JSON.stringify(obj, null, 2));
    process.exit(0);
};

return send(testMessage, finalRcvFunc);
