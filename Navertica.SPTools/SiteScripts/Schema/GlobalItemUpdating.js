(function () {
    window.JSONschemas["GlobalItemUpdating"] = function (contents, force) {
        if (!window.JSONschemas || !window.loadSchema) {
            console.log("missing global functions and objects - running in wrong context?");
            return null;
        }
        var schema = {
            "properties": {
                "Event": {
                    "type": "string",
                    "je:hint": "readonly",
                    "title": "Event",
                    "default": "GlobalItemUpdating"
                }
            }
        };

        return schema;
    };
})();