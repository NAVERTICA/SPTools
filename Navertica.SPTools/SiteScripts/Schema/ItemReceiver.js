(function () {
    window.JSONschemas["ItemReceiver"] = function (contents, title, force) {
        if (!window.JSONschemas || !window.loadSchema) {
            console.log("missing global functions and objects - running in wrong context?");
            return null;
        }
        var schema = {
            "type": "object",
            //"title": "title",
            "additionalProperties": {
                "type": "string", "je:hint": "schApp",
                "je:schApp": {
                    "id": "-App",
                    "path": ["/_layouts/15/NVR.SPTools/"],
                    "addons": ["/_layouts/15/NVR.SPTools/libs/jquery-ui.js", "/_layouts/15/NVR.SPTools/libs/jquery.multiselect.min.js", "/_layouts/15/NVR.SPTools/libs/jquery.multiselect.filter.min.js"],
                    "styles": ["/_layouts/15/NVR.SPTools/libs/uithemes/smoothness/jquery-ui.css", "/_layouts/15/NVR.SPTools/libs/jquery.multiselect.css", "/_layouts/15/NVR.SPTools/libs/jquery.multiselect.filter.css"]
                }
            },
            "order": [],
            "properties": {}
        };

        if (!contents || Object.prototype.toString.call(contents) !== "[object Object]") {
            console.log("weird contents, replacing with empty object", contents);
            contents = {};
        }
        var success = true;
        /*$.each(contents, function (scriptName, configName) {
            var pluginSchemaName = configName.split("|")[0];
            try {
                if (!success) return;
                if (!window.JSONschemas[pluginSchemaName] || force) loadSchema(k, v);
                if (!window.JSONschemas[k]) {
                    success = false;
                    return;
                }
                schema.properties[k] = window.JSONschemas[k](v);

                if (schema.properties[k]) schema.order.push(k);
            } catch (e) {
                console.error("ItemReceiver - loading schema");
                success = false;
            }
        });*/

        if (!success) return null;
        return schema;
    };
})();
