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

    window["NVRAddAppEntries"]["DuplicateFields"] =
    {
        "Name": "DuplicateFields",
        "IconUrl": "/_layouts/15/images/ltdl.png?rev=23",
        "AppInstallUrl": "/Lists/SiteConfig/NewForm.aspx?"
            + "NVR_SiteConfigActive=true&NVR_SiteConfigActiveFor=" + encodeURIComponent(ContextGuids.userlogin) /* TODO old SPTools */
            + "&NVR_SiteConfigApp=DuplicateFields--I%0AItemUpdated%0AItemAdded&Title=DuplicateFields%20-%20/" + relSource
            + "&NVR_SiteConfigPackage=Receivers"
            + "&Source=" + tmpsrc
            + "&NVR_SiteConfigUrl=/" + relSource
            + "&NVR_SiteConfigJSON=" + encodeURIComponent('{"ItemUpdated": {"DuplicateFields--I": "DuplicateFields"},"ItemAdded": {"DuplicateFields--I": "DuplicateFields"} }')
            + "&ContentTypeId=0x010000AF00A19FBA41D68E7FDB37BCB1847B",
        "AppDetailsUrl": 'javascript:SP.UI.ModalDialog.showModalDialog({' +
            'url: "/_layouts/15/NVR.SPTools/GetScript.aspx?FilePaths=/NVR/DuplicateFields/Info.html",' +
            'width: 800,' +
            'height: 600,' +
            'allowMaximize: true});'
    };
})();