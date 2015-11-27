(function () {
	window.JSONschemas["schSingleUrl"] = function(contents, force) {
		if (!window.JSONschemas || !window.loadSchema) {
			console.log("missing global functions and objects - running in wrong context?");
			return null;
		}
var schema =
{"properties": {	
"FROM-FIELD":{
	"type": "string",
	"je:hint": "schField",
	"default": "Very Much Empty",
	"je:schField": {
		"id":"veryspecificIDFIELD",
		"fromURLField":"anoProsim",
		"path": "",
		"addons": ["/_layouts/15/SPTools/libs/jquery-ui.js", "/_layouts/15/SPTools/libs/jquery.multiselect.min.js", "/_layouts/15/SPTools/libs/jquery.multiselect.filter.min.js"],
		"scripts": [],
		"styles": ["/_layouts/15/SPTools/libs/uithemes/smoothness/jquery-ui.css", "/_layouts/15/SPTools/libs/jquery.multiselect.css", "/_layouts/15/SPTools/libs/jquery.multiselect.filter.css"],
		"startfn": "",
	}
},
"TO-FIELD":{
	"type": "string",
	"je:hint": "schField",
	"default": "Very Much Empty",
	"je:schField": {
		"id":"veryspecificIDFIELD2",
		"fromURLField":"anoProsim",
		"path": "",
		"addons": [],
		"scripts": [],
		"styles": [],
		"startfn": "",
	}
},
	}
};
		return schema;
	};
})();