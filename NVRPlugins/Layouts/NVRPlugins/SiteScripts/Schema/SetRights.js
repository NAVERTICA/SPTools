(function () {
    window.JSONschemas["SetRights"] = function (contents, force) {
        if (!window.JSONschemas || !window.loadSchema) {
            console.log("missing global functions and objects - running in wrong context?");
            return null;
        }
        var schema =
        {
            "type": "object",
            "properties": {
                "Debug": {
                    "type": "boolean",
                    "title": "Debug"
                },
                "EventFiringEnabled": {
                    "type": "boolean",
                    "title": "EventFiringEnabled"
                },
                "Rights": {
                    "additionalProperties": {
                        "type": "object",
                        "properties": {
                            "RightsFor": {
                                "title": "RightsFor",
                                "type": "oneOf",
                                "id": "-oneOf",
                                "oneOf": [
                                    {
                                        "$ref": "#/definitions/self"
                                    },
                                    {
                                        "$ref": "#/definitions/LookupField"
                                    }
                                ],
                                "definitions": {
                                    "self": {
                                        "type": "string",
                                        "pattern": "^self$",
                                        "je:hint": "readonly",
                                        "default": "self"
                                    },
                                    "LookupField": {
                                        "type": "string",
                                        "pattern": "^((?!self).).*$",
                                        "je:hint": "schFieldString",
                                        "default": "",
                                        "je:schFieldString": {
                                            "id": "",
                                            "fromURLField": "ano",
                                            "followLookup": true,
                                            "loadTypes": "Lookup",
                                            "addons": [],
                                            "styles": []
                                        }
                                    }
                                }
                            },
                            "Clear": {
                                "type": "boolean",
                                "title": "Clear"
                            },
                            "ExpandUsers": {
                                "type": "boolean",
                                "title": "ExpandUsers"
                            },
                            "Roles": {
                                "type": "array",
                                "items": {
                                    "type": "object",
                                    "properties": {
                                        "Role_Name": {
                                            "type": "string",
                                            "title": "Role Name",
                                            "je:hint": "schRoles",
                                            "je:schRoles": {
                                                "addons": [],
                                                "styles": []
                                            }
                                        },
                                        "For": {
                                            "type": "array",
                                            "items": {
                                                "type": "oneOf",
                                                "id": "-oneOf",
                                                "oneOf": [
                                                    {
                                                        "$ref": "#/definitions/schUser"
                                                    },
                                                    {
                                                        "$ref": "#/definitions/schField"
                                                    }
                                                ],
                                                "definitions": {
                                                    "schUser": {
                                                        "type": "string",
                                                        "pattern": ".*(\\\\).*",
                                                        "je:hint": "schUser",
                                                        "default": "",
                                                        "je:schUser": {}
                                                    },
                                                    "schField": {
                                                        "type": "string",
                                                        "pattern": "^[^:#\\\\]*$",
                                                        "je:hint": "schFieldString",
                                                        "default": "",
                                                        "je:schFieldString": {
                                                            "id": "",
                                                            "fromURLField": "ano",
                                                            "followLookup": true,
                                                            "addons": [],
                                                            "styles": []
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
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