// aplikace, ktera chce pridat svou ikonku do seznamu aplikaci k pridani, musi 
// do window["NVRAddAppEntries"] zapsat do vlastniho klice svou konfiguraci
// tzn. window["NVRAddAppEntries"]["TestApp"] = { "Name": "", "IconUrl":"", "AppInstallUrl":"", "AppDetailsUrl":"" };
// zarazenim tohoto skriptu se vytvori prazdne window["NVRAddAppEntries"] a po nacteni core.js se funkce na pridavani ikonek spusti
if (!window["NVRAddAppEntries"]) window["NVRAddAppEntries"] = { 
//"testApp": { "Name": "Test App Name", "IconUrl": "http://portal13jmt/_layouts/15/images/ltdl.png?rev=23", "AppInstallUrl": "http://portal13jmt", "AppDetailsUrl": "http://portal13jmt/#testappdetails" }
};

function addNVRAppEntries() {
    if (!String.format) {
        String.format = function (format) {
            var args = Array.prototype.slice.call(arguments, 1);
            return format.replace(/{(\d+)}/g, function (match, number) {
                return typeof args[number] != 'undefined'
                  ? args[number]
                  : match
                ;
            });
        };
    }

    // Navertica section
    if ($("#idStorefrontHeaderNVR").length == 0 && window["NVRAddAppEntries"].length != 0) {
        $("#idStorefrontHeader").append(
            '<div id="idStorefrontHeaderNVR" style="width: 736px; overflow: hidden;">'
            + '<div class="ms-storefront-mngsubtitle ms-webpart-titleText">NAVERTICA addons</div><div class="ms-storefront-clear"></div>'
            + '<div id="scrollDiv" style="left: 0px; width: 552px; position: relative;"><ul class="ms-storefront-list" id="idStorefrontListNVRAppIcons" role="listbox"></ul></div>');
    }

    $.each(window["NVRAddAppEntries"], function(key, value) {
        var appName = value["Name"];
        var appIcon = value["IconUrl"];
        var appInstallUrl = value["AppInstallUrl"];
        var appDetailsUrl = value["AppDetailsUrl"];

        var appEntryLi = $(String.format('<li class="ms-storefront-listitem ms-storefront-myappicon" id="idStorefrontMyAppIconMyAppIcons" role="option">'
            + '<div class="ms-storefront-myappicondiv"><a tabindex="-1" class="ms-storefront-selectanchor ms-storefront-appiconspan" id="idStorefrontMyAppIconImage" href="{2}">'
            + '<img width="212" height="183" class="ms-storefront-appiconimg" alt="" src="{1}">'
            + '</a></div><div class="ms-storefront-myappicontop"><a class="ms-storefront-selectanchor" href="{2}">'
            + '<div title="{0}" class="ms-storefront-myappicontext ms-storefront-wordwrap" id="idStorefrontMyAppIconText">{0}</div></a>'
            + '<a class="ms-textSmall" id="idStorefrontMyAppIconButton" href="{3}">App Details</a></div></li>',
            appName,
            appIcon,
            appInstallUrl,
            appDetailsUrl
            ));

        $("#idStorefrontHeaderNVR").append(appEntryLi);
    });
}

ExecuteOrDelayUntilScriptLoaded(function() {
    setTimeout(function() { addNVRAppEntries(); }, 1000);
}, "sp.storefront.js"); 