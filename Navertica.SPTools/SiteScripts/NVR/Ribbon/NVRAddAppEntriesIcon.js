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

    window["NVRAddAppEntries"]["Ribbon"] =
    {
        "Name": "Ribbon",
        "IconUrl": "/_layouts/15/images/lttask.png?rev=23",
        "AppInstallUrl": "/Lists/SiteConfig/NewForm.aspx?"
            + "NVR_SiteConfigApp=Ribbon%0AJavaScripts&Title=Ribbon%20-%20/" + relSource
            + "&NVR_SiteConfigPackage=Ribbon"
            + "&Source=" + tmpsrc
            + "&NVR_SiteConfigUrl=/" + relSource
            + "&NVR_SiteConfigJSON=" + encodeURIComponent('{"JavaScripts": {"LibLinks": [""],"ScriptLinks": ["JS/createbuttons.js"]}}')
            + "&ContentTypeId=0x010000AF00A19FBA41D68E7FDB37BCB1847B",
        "AppDetailsUrl": 'javascript:SP.UI.ModalDialog.showModalDialog({' +
            'url: "/_layouts/15/NVR.SPTools/GetScript.aspx?FilePaths=/NVR/Ribbon/Info.html",' +
            'width: 800,' +
            'height: 600,' +
            'allowMaximize: true});'
    };
})();