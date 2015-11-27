(function ()
{
	window.JSONschemas["FormIterator"] = function(contents, force)
	{
        if (!window.JSONschemas || !window.loadSchema) {
            console.log("missing global functions and objects - running in wrong context?");
            return null;
        }

        var schema = 
{
    "type": "object",
    "properties": {
        "Plugins": {
            "type": "object",
            "properties": {
                "Prefill": {
                    "type": "string"
                },
                "Nonedit": {
                    "type": "string"
                },
                "Exclude": {
                    "type": "string"
                }
            }
        },
        "FormTemplates": {
            "type": "object",
            "properties": {
                "NewForm": {
                    "type": "string"
                },
                "EditForm": {
                    "type": "string"
                },
                "DisplayForm": {
                    "type": "string"
                }
            }
        },
        "Tabs": {
            "type": "array",
            "items": {
                "type": "object",
                "properties": {
                    "Title": {
                        "type": "array",
                        "items": {
                            "type": "object",
                            "properties": {
                                "Language": {
                                    "type": "string",
                                    "enum": [
                                        "cs-CZ",
                                        "en-US",
                                        "ru-RU",
                                        "sk-SK"
                                    ]
                                },
                                "Title": {
                                    "type": "string"
                                }
                            }
                        }
                    },
                    "Url": {
                        "type": "string"
                    },
                    "Order": {
                        "type": "integer"
                    },
                    "LookupField": {
                        "type": "string"
                    }
                },
                "required": [
                    "Order",
                    "Url"
                ]
            }
        }
    }
}
	return schema;
};

})();
