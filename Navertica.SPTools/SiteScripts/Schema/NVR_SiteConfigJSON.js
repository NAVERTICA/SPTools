// javascript s funkci, ktera ma vracet JSON schema, a kterou skript musi zapsat do window.JSONschemas
(function () {
    window.JSONschemas["NVR_SiteConfigJSON"] = function (contents, force) {
        if (!window.JSONschemas || !window.loadSchema) {
            console.log("missing global functions and objects - running in wrong context?");
            return null;
        }
        console.log("NVR_SiteConfigJSONschema generator with contents\n", contents);
        var schema = {
            "je:hint": "tabs",
            "title": "App tabs",
            "order": [],
            "id": "NVR_SiteConfigJSON.schema",
            "type": "object",
            "additionalProperties": { "type": "object" },
            "properties": {}
        };

        var apps = $("textarea[title='Apps']").val();
        var divider = "\n";
        var schemaDivider = '--';
        var possibleDividers = ["\n", ",", ";"];

        for (var div in possibleDividers) {
            if (apps.indexOf(possibleDividers[div]) != -1) {
                divider = possibleDividers[div];
                break;
            }
        }

        // nacteme schemata pro jednotlive aplikace v App radku SiteConfigu
        // a jednotliva schemata si pak nacitaji a do sebe vkladaji svoje konfigurace
        var splitApps = [apps];
        if (apps.indexOf(schemaDivider) != -1) { splitApps = [apps.split(schemaDivider)[1]]; }
        if (divider) splitApps = apps.split(divider);
        if (divider && apps.indexOf(schemaDivider) != -1) {
            splitApps = [];
            $(apps.split(divider)).each(function (ind, val) {
                $(val.split(schemaDivider)).each(function (i, el) {
                    if (i == 0) splitApps.push(this.toString());
                });
            });
        }
        console.log("splitApps", splitApps);
        var success = true;
        for (var app in splitApps) {
            if (!success) break;
            var a = splitApps[app].trim();
            var b = apps.split(divider)[app]/*.replace(schemaDivider,"-")*/;

            if (!window.JSONschemas[a] || force) {
                console.log("loading NVR_SiteConfigJSON subschema ", a, " with json contents\n", contents[a]);
                success = loadSchema(a, contents[a]);
                if (!success) console.error("error loading NVR_SiteConfigJSON subschema ", a);
            }

            schema.properties[b] = window.JSONschemas[a](contents[a]);

            if (schema.properties[b]) schema.order.push(b);
        }

        if (!success) return null;
        return schema;
    };


})();