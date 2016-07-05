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

var logFunc = function (error, response, body) {
    if (error) {
	console.log("Error: " + error);
    }
    else if (response.statusCode != 200) {
	console.log("Status: " + response.statusCode);
	console.log("Response: " + JSON.stringify(response, null, 2));
    }
    console.log(body);
};

var tests = [
    {
	"uri": citiesHost + '/managers',
	"method": "GET"
    },
    {
	"uri": citiesHost + '/managers/CitizenManager/call/createCitizen',
	"method": "POST",
	"content-type": "application/json",
	"data": JSON.stringify(obj)
    },
    {
	"uri": testHost + '/managers',
	"method": "GET"
    },
    {
	"uri": testHost + '/call',
	"method": "POST",
	"content-type": "application/json",
	"data": JSON.stringify(obj)
    },
    {
	"uri": testHost + '/managers/CitizenManager/call/CreateCitizen',
	"method": "POST",
	"content-type": "text/plain",
	"data": JSON.stringify(obj)
    },
];

for (var i in tests) {
    var t = tests[i];
    var options = {
	method: t.method,
	uri: t.uri,
	headers: {
	    'content-type': t["content-type"]+'; charset=utf-8'
	},
    };
    if (t.data)
	options.body = t.data;

    request(
	options,
	logFunc
    );
}

