(function () {
    window.JSONschemas["Translations"] = function(contents, force) {
        if (!window.JSONschemas || !window.loadSchema) {
            console.log("missing global functions and objects - running in wrong context?");
            return null;
        }
var schema =
{
	"type":"object",
	"properties":{
		"Hlasky":{
			"additionalProperties":{
				"type": "object",
				"properties":{
					"1029":{
						"title":"[1029] Czech",
						"type":"string",
					},
					"1051":{
						"title":"[1051] Slovak",
						"type":"string",
					},
					"1033":{
						"title":"[1033] English",
						"type":"string",
					},
					"1049":{
						"title":"[1049] Russian",
						"type":"string",
					},
				}
			}
		}
	}
};
		return schema;
    };
})();