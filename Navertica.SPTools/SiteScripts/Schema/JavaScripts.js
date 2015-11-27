(function () {
    window.JSONschemas["JavaScripts"] = function(contents, force) {
        if (!window.JSONschemas || !window.loadSchema) {
            console.log("missing global functions and objects - running in wrong context?");
            return null;
        }
        var schema = {
            "id": "JavaScripts.schema",
            "required": false,
            "type": "object",
            "properties": {
                "LibLinks": {
                    "id": "LibLinks",
                    "required": false,
                    "type": "array",
                    "items":
                    {
                        "id": "LibLinksItems",
                        "required": false,
                        "type": "string"
                    }
                },
                "ScriptLinks": {
                    "id": "ScriptLinks",
                    "required": false,
                    "type": "array",
                    "items":
                    {
                        "id": "ScriptLinksItems",
                        "required": false,
                        "type": "string"
                    }
                }
            }
        };

        return schema;
    };
})();


