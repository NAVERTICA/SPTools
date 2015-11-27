(function () {
    window.JSONschemas["DuplicateFields"] = function (contents, force) {
        if (!window.JSONschemas || !window.loadSchema) {
            console.log("missing global functions and objects - running in wrong context?");
            return null;
        }
        var schema =
        {
            "properties": {
                "DuplicateFields": {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "FROM-FIELD": {
                                "type": "array",
                                "je:hint": "schField",
                                "default": "Empty",
                                "je:schField": {
                                    "id": "duplicateFromField",
                                    "fromURLField": "Yes",
                                    "path": "",
                                    "addons": [],
                                    "styles": [],
                                    "scripts": [],
                                    "startfn": "",
                                }
                            },
                            "TO-FIELD": {
                                "type": "array",
                                "je:hint": "schField",
                                "default": "Empty",
                                "je:schField": {
                                    "id": "duplicateToField",
                                    "fromURLField": "Vskutku",
                                    "path": "",
                                    "addons": [],
                                    "scripts": [],
                                    "styles": [],
                                    "startfn": "",
                                }
                            }
                        }
                    }
                }
            }
        };
        return schema;
    };
})();