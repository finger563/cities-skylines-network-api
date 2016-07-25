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
    'Method': 'GET',
    'Object': {
	'Name': 'GetTrafficLightState',
	'Type': 'METHOD',
	'IsStatic': false,
	'Parameters': [
	    {
		'Type': 'PARAMETER',
		'Value': 154,
		'ValueType': 'System.UInt32'
	    },
	    {
		'Type': 'MEMBER',
		'Name': 'ElementAt',
		'IsStatic': false,
		'Parameters': [
		    {
			'Type': 'PARAMETER',
			'Value': 0,
			'ValueType': 'System.UInt32'
		    }
		],
		'Dependency': {
		    'Name': 'm_buffer',
		    'Type': 'MEMBER',
		    'IsStatic': false,
		    'Dependency': {
			'Name': 'm_segments',
			'Type': 'MEMBER',
			'IsStatic': false,
			'Dependency': {
			    'Name': 'instance',
			    'Type': 'MEMBER',
			    'IsStatic': true,
			    'Dependency': {
				'Type': 'CLASS',
				'Name': 'NetManager',
				'Assembly': 'Assembly-CSharp'
			    }
			}
		    }
		}
	    },
	    {
		'Name': 'm_currentFrameIndex',
		'Type': 'MEMBER',
		'IsStatic': false,
		'Dependency': {
		    'Name': 'instance',
		    'IsStatic': true,
		    'Type': 'MEMBER',
		    'Dependency': {
			'Name': 'SimulationManager',
			'Assembly': 'Assembly-CSharp',
			'Type': 'CLASS'
		    }
		}
	    },
	    {
		'Type': 'PARAMETER',
		'Value': 'GreenToRed',
		'ValueType': 'RoadBaseAI+TrafficLightState',
		'Assembly': 'Assembly-CSharp'
	    },
	    {
		'Type': 'PARAMETER',
		'Value': 'GreenToRed',
		'ValueType': 'RoadBaseAI+TrafficLightState',
		'Assembly': 'Assembly-CSharp'
	    },
	    {
		'Type': 'PARAMETER',
		'Value': false,
		'ValueType': 'System.Boolean'
	    },
	    {
		'Type': 'PARAMETER',
		'Value': false,
		'ValueType': 'System.Boolean'
	    }
	],
	'Dependency': {
	    'Name': 'RoadBaseAI',
	    'Assembly': 'Assembly-CSharp',
	    'Type': 'CLASS'
	}
    }
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

var getStateTest = {
    'Method': 'GETSTATE',
    'Object': {
	'Name': 'NodeId',
	'Type': 'PARAMETER',
	'Value': 0,  // should be 0 - 3 (for the selected ids)
	'ValueType': 'System.UInt32'
    }
};

var getDensityTest = {
    'Method': 'GETDENSITY',
    'Object': {
	'Name': 'NodeId',
	'Type': 'PARAMETER',
	'Value': 0,  // should be 0 - 3 (for the selected ids)
	'ValueType': 'System.UInt32',
	'Parameters': [
	    {
		'Name': 'SegmentId',
		'Type': 'PARAMETER',
		'Value': 0,
		'ValueType': 'System.UInt32'
	    }
	]
    }
};

var setStateTest = {
    'Method': 'SETSTATE',
    'Object': {
	'Name': 'NodeId',
	'Type': 'PARAMETER',
	'Value': 0,  // should be 0 - 3 (for the selected ids)
	'ValueType': 'System.UInt32'
    }
};

var test = frameTest;

if (process.argv[2]) {
    if (process.argv[2] == "buffer") {
	test = bufferTest;
    }
    else if (process.argv[2] == "method") {
	test = methodTest;
    }
    else if (process.argv[2] == "int") {
	test = intTest;
    }
    else if (process.argv[2] == "enum") {
	test = enumTest;
    }
    else if (process.argv[2] == "getState") {
	if (process.argv[3])
	    getStateTest.Object.Value = process.argv[3];
	test = getStateTest;
    }
    else if (process.argv[2] == "getDensity") {
	if (process.argv[3])
	    getDensityTest.Object.Value = process.argv[3];
	if (process.argv[4])
	    getDensityTest.Object.Parameters[0].Value = process.argv[4];
	test = getDensityTest;
    }
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
