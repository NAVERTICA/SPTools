(function() {
    if (!window["NVRAddAppEntries"]) window["NVRAddAppEntries"] = {};

    var tmpsrc = NVR.common.get_url_param("Source");
    var relSource = NVR.getUrlParts(decodeURIComponent(tmpsrc)).pathname;

    window["NVRAddAppEntries"]["LinkedLookup"] =
    {
        "Name": "LinkedLookup",
        "IconUrl": "/_layouts/15/images/ltdl.png?rev=23",
        "AppInstallUrl": "/Lists/SiteConfig/NewForm.aspx?"
            + "NVR_SiteConfigActive=true"
            + "&NVR_SiteConfigApp=LinkedLookup--I%0AItemUpdating%0AItemAdded%0AItemDeleting&Title=LinkedLookup%20-%20/" + relSource
            + "&NVR_SiteConfigPackage=Receivers"
            + "&Source=" + tmpsrc
            + "&NVR_SiteConfigUrl=/" + relSource
            + "&NVR_SiteConfigJSON=" + encodeURIComponent('{"ItemUpdating": {"LinkedLookup--I": "LinkedLookup"},"ItemAdded": {"LinkedLookup--I": "LinkedLookup"},"ItemDeleting": {"LinkedLookup--I": "LinkedLookup"} }')
            + "&ContentTypeId=0x010000AF00A19FBA41D68E7FDB37BCB1847B",
        "AppDetailsUrl": 'javascript:SP.UI.ModalDialog.showModalDialog({' +
            'url: "/_layouts/15/NVR.SPTools/GetScript.aspx?FilePaths=/NVR/LinkedLookup/Info.html",' +
            'width: 800,' +
            'height: 600,' +
            'allowMaximize: true});'
    };
})();