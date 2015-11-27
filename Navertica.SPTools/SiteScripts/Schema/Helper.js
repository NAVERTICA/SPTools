(function ()
{
	window.JSONschemas["Helper"] = function(contents, force)
	{
        if (!window.JSONschemas || !window.loadSchema) {
            console.log("missing global functions and objects - running in wrong context?");
            return null;
        }

        var schema = 
{
    "type": "object",
    "properties": {
        "Theme": {
            "type": "string",
            "default": "introjs-dark"
        },
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
        "Steps": {
            "type": "array",
            "items": {
                "type": "object",
                "properties": {
                    "Intro": {
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
                    "Position": {
                        "type": "string",
                        "enum": [
                            "top",
                            "left",
                            "right",
                            "bottom",
                            "bottom-left-aligned",
                            "bottom-middle-aligned",
                            "bottom-right-aligned"
                        ]
                    },
                    "Element": {
                        "type": "string"
                    }
                }
            }
        }
    },
    "required": [
        "Title",
        "Steps"
    ]
}
	
	return schema;
};

})();
