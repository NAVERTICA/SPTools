(function () {
    window.JSONschemas["DerivedAction"] = function(contents, force) {
        if (!window.JSONschemas || !window.loadSchema) {
            console.log("missing global functions and objects - running in wrong context?");
            return null;
        }
        var schema = {
            "type": "string",
            "title": "DerivedAction"
        };

        return schema;
    };
})();
