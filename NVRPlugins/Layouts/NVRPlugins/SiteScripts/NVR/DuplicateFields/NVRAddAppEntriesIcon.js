(function() {
    if (!window["NVRAddAppEntries"]) window["NVRAddAppEntries"] = {};

    var appName = "DuplicateFields";
    var connectedApps = ["ItemUpdated", "ItemAdded"]; // []
    var packageName = "Receivers";
    var contentTypeId = "";
    var infoFilePath = "/NVR/DuplicateFields/Info.html";
    var minimalJSONconfig = '{"ItemUpdated": {"DuplicateFields--I": "DuplicateFields"},"ItemAdded": {"DuplicateFields--I": "DuplicateFields"} }';


    var tmpsrc = NVR.common.get_url_param("Source");
    var relSource = NVR.getUrlParts(decodeURIComponent(tmpsrc)).pathname;
    
    window["NVRAddAppEntries"][appName] =
    {
        "Name": appName,
        "IconUrl": "/_layouts/15/images/ltdl.png?rev=23",
        "AppInstallUrl": "/Lists/SiteConfig/NewForm.aspx?"
            + "NVR_SiteConfigActive=true"
            + "&NVR_SiteConfigApp=" + appName + connectedApps.join("%0A") + "&Title=" + appName + "%20-%20/" + relSource
            + "&NVR_SiteConfigPackage=" + packageName
            + "&Source=" + tmpsrc
            + "&NVR_SiteConfigUrl=/" + relSource
            + "&NVR_SiteConfigJSON=" + encodeURIComponent(minimalJSONconfig)
            + "&ContentTypeId=" + contentTypeId,
        "AppDetailsUrl": 'javascript:SP.UI.ModalDialog.showModalDialog({' +
            'url: "/_layouts/15/NVR.SPTools/GetScript.aspx?FilePaths=' + infoFilePath + '",' +
            'width: 800,' +
            'height: 600,' +
            'allowMaximize: true});'
    };
})();