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

var citiesOptions = {
    method: 'POST',
    uri: citiesHost + '/testMethod',
    headers: {
	'content-type': 'application/json; charset=utf-8'
    },
    body: JSON.stringify(obj)
};

var testOptions = _.clone(citiesOptions);
testOptions.uri = testHost + '/managers/CitizenManager/call/CreateCitizen';

request(
    citiesOptions,
    logFunc
);

request(
    testOptions,
    logFunc
);

