(function () {
    window.JSONschemas["LinkedLookup"] = function (contents, force) {
        if (!window.JSONschemas || !window.loadSchema) {
            console.log("missing global functions and objects - running in wrong context?");
            return null;
        }
        var schema =
        {
            "type": "object",
            "properties": {
                "LinkedLookup": {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "RegularUpdate": {
                                "type": "boolean",
                                "title": "RegularUpdate"
                            },
                            "Force": {
                                "type": "boolean",
                                "title": "Force"
                            },
                            "UpdateVersion": {
                                "type": "boolean",
                                "title": "UpdateVersion"
                            },
                            "LocalWeb": {
                                "type": "string",
                                "je:hint": "schWeb",
                                "je:schWeb": {
                                    "id": "LocalIDWEB",
                                    "childID": "LocalIDLIST",
                                    "path": "",
                                    "addons": [],
                                    "scripts": [],
                                    "styles": [],
                                    "startfn": ""
                                }
                            },
                            "LocalList": {
                                "type": "string",
                                "je:hint": "schList",
                                "je:schList": {
                                    "id": "LocalIDLIST",
                                    "parentID": "LocalIDWEB",
                                    "childID": "LocalIDFIELD",
                                    "path": "",
                                    "addons": [],
                                    "scripts": [],
                                    "styles": [],
                                    "startfn": ""
                                }
                            },
                            "LocalField": {
                                "type": "string",
                                "je:hint": "schFieldString",
                                "je:schFieldString": {
                                    "id": "LocalIDFIELD",
                                    "parentID": "LocalIDLIST",
                                    "path": "",
                                    "addons": [],
                                    "scripts": [],
                                    "styles": [],
                                    "startfn": ""
                                }
                            },
                            "RemoteWeb": {
                                "type": "string",
                                "je:hint": "schWeb",
                                "je:schWeb": {
                                    "id": "RemoteIDWEB",
                                    "childID": "RemoteIDLIST",
                                    "path": "",
                                    "addons": [],
                                    "scripts": [],
                                    "styles": [],
                                    "startfn": ""
                                }
                            },
                            "RemoteList": {
                                "type": "string",
                                "je:hint": "schList",
                                "je:schList": {
                                    "id": "RemoteIDLIST",
                                    "parentID": "RemoteIDWEB",
                                    "childID": "RemoteIDFIELD",
                                    "path": "",
                                    "addons": [],
                                    "scripts": [],
                                    "styles": [],
                                    "startfn": ""
                                }
                            },
                            "RemoteField": {
                                "type": "string",
                                "je:hint": "schFieldString",
                                "je:schFieldString": {
                                    "id": "RemoteIDFIELD",
                                    "parentID": "RemoteIDLIST",
                                    "path": "",
                                    "addons": [],
                                    "scripts": [],
                                    "styles": [],
                                    "startfn": ""
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