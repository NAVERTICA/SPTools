// Button Element https://msdn.microsoft.com/en-us/library/office/ff458366.aspx

(function () {
    window.JSONschemas["Ribbon"] = function(contents, force) {
        if (!window.JSONschemas || !window.loadSchema) {
            console.log("missing global functions and objects - running in wrong context?");
            return null;
        }
        var schema =
            {
                "type": "object",
                "$schema": "http://json-schema.org/draft-03/schema",
                "id": "http://jsonschema.net",
                "required": false,
                "properties": {
                     "controlProperties": {
                         "type": "array",
                         "id": "http://jsonschema.net/controlProperties",
                         "required": false,
                         "description": "test array description",
                         "items": {
                             "type": "object",
                             "id": "http://jsonschema.net/controlProperties/0",
                             "required": false,
                             "properties": {
                                  "actionScript": {
                                      "type": "string",
                                      "title": "Action JavaScript call",
                                      "description": "Action JS description",
                                      "id": "http://jsonschema.net/controlProperties/0/actionScript",
                                      "required": false
                                  },
                                  "buttonID": {
                                      "type": "string",
                                      "title": "Button HTML id",
                                      "id": "http://jsonschema.net/controlProperties/0/buttonID",
                                      "required": false
                                  },
                                  "commandID": {
                                      "type": "string",
                                      "title": "The name of the command/action",
                                      "id": "http://jsonschema.net/controlProperties/0/commandID",
                                      "required": false
                                  },
                                  "controlPropertiesID": {
                                      "type": "string",
                                      "title": "Control properties ID",
                                      "id": "http://jsonschema.net/controlProperties/0/controlPropertiesID",
                                      "required": false
                                  },
                                  "enabledScript": {
                                      "type": "string",
                                      "title": "Button enabled JavaScript call",
                                      "id": "http://jsonschema.net/controlProperties/0/enabledScript",
                                      "required": false
                                  },
                                  "group": {
                                      "type": "string",
                                      "title": "Group name",
                                      "id": "http://jsonschema.net/controlProperties/0/group",
                                      "required": false
                                  },
                                  "image16": {
                                      "type": "string",
                                      "title": "Icon 16x16",
                                      "id": "http://jsonschema.net/controlProperties/0/image16",
                                      "required": false
                                  },
                                  "image32": {
                                      "type": "string",
                                      "title": "Icon 32x32",
                                      "id": "http://jsonschema.net/controlProperties/0/image32",
                                      "required": false
                                  },
                                  "labelText": {
                                      "type": "string",
                                      "title": "Label text",
                                      "id": "http://jsonschema.net/controlProperties/0/labelText",
                                      "required": false
                                  },
                                  "rowNumber": {
                                      "type": "string",
                                      "title": "Row number",
                                      "id": "http://jsonschema.net/controlProperties/0/rowNumber",
                                      "required": false
                                  },
                                  "sectionId": {
                                      "type": "string",
                                      "title": "Section ID",
                                      "id": "http://jsonschema.net/controlProperties/0/sectionId",
                                      "required": false
                                  },
                                  "sectionRows": {
                                      "type": "string",
                                      "title": "Section rows",
                                      "id": "http://jsonschema.net/controlProperties/0/sectionRows",
                                      "required": false
                                  },
                                  "tab": {
                                      "type": "string",
                                      "title": "Tab title",
                                      "id": "http://jsonschema.net/controlProperties/0/tab",
                                      "required": false
                                  },
                                  "toolTipTitle": {
                                      "type": "string",
                                      "title": "Tooltip title",
                                      "id": "http://jsonschema.net/controlProperties/0/toolTipTitle",
                                      "required": false
                                  },
                                  "toolTipDescription": {
                                      "type": "string",
                                      "title": "Tooltip description",
                                      "id": "http://jsonschema.net/controlProperties/0/toolTipDescription",
                                      "required": false
                                  },
                                  "toolTipHelpKeyWord": {
                                      "type": "string",
                                      "title": "Tooltip Help Keyword",
                                      "id": "http://jsonschema.net/controlProperties/0/toolTipHelpKeyWord",
                                      "required": false
                                  },
                                  "toolTipShortcutKey": {
                                      "type": "string",
                                      "title": "Tooltip Shortcut Key (ie. 'ALT + J')",
                                      "id": "http://jsonschema.net/controlProperties/0/toolTipShortcutKey",
                                      "required": false
                                  },
                                  
                             }
                         }
                     },
                    "hideSelector": {
                        "type": "string",
                        "title": "Selector to hide buttons/sections",

                    }
                }
}





        ;

        return schema;
    };
})();