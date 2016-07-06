var request = require('request');
var _ = require('underscore');

var citiesHost = 'http://localhost:8080';
var testHost = 'http://localhost:4040';

var obj = [
    {
	"Name": "name",
	"Type": "string",
	"Value": "testName"
    },
    {
	"Name": "name2",
	"Type": "boolean",
	"Value": "this is another value"
    }
];

obj = {
    "Name": "name2",
    "Type": "boolean",
    "Value": "this is another value",
    "arr": [
	{
	    "name": "object1"
	},
	{
	    "name": "object2"
	}
    ]
};


var tests = [

    {
	"uri": citiesHost + '/managers',
	"method": "GET"
    },
    {
	"uri": citiesHost + '/managers/CitizenManager/call/CreateCitizen?params='+JSON.stringify(obj),
	"method": "GET",
    },
    {
	"uri": testHost + '/managers',
	"method": "GET"
    },
    {
	"uri": testHost + '/managers/CitizenManager/call/CreateCitizen?params='+JSON.stringify(obj),
	"method": "GET",
    },
];

tests.map((t) => {
    var options = {
	method: t.method,
	uri: t.uri
    };
    if (t["content-type"])
	options.headers['content-type'] = t["content-type"]+'; charset=utf-8';
    if (t.data)
	options.body = t.data;

    request(
	options,
	function (error, response, body) {
	    console.log('\n\nConnected to ' + t.uri + ':');
	    console.log('------');
	    if (error) {
		console.log(error);
		return;
	    }
	    else if (response.statusCode != 200) {
		console.log("Status: " + response.statusCode);
		console.log("Response: " + JSON.stringify(response, null, 2));
		return;
	    }
	    //var testObj = JSON.parse(body);
	    console.log(body);
	}
    );
});

