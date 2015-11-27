(function() {
    if (!window["NVRAddAppEntries"]) window["NVRAddAppEntries"] = {};


    function getUrlParts(url) {
        var a = document.createElement('a');
        a.href = url;

        return {
            href: a.href,
            host: a.host,
            hostname: a.hostname,
            port: a.port,
            pathname: a.pathname,
            protocol: a.protocol,
            hash: a.hash,
            search: a.search
        };
    }

    var tmpsrc = get_url_param("Source");
    var relSource = getUrlParts(decodeURIComponent(tmpsrc)).pathname;
    console.log("relSource", relSource);

    window["NVRAddAppEntries"]["LinkedLookup"] =
    {
        "Name": "LinkedLookup",
        "IconUrl": "/_layouts/15/images/ltdl.png?rev=23",
        "AppInstallUrl": "/Lists/SiteConfig/NewForm.aspx?"
            + "NVR_SiteConfigActive=true&NVR_SiteConfigActiveFor=" + encodeURIComponent(ContextGuids.userlogin) /* TODO old SPTools */
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