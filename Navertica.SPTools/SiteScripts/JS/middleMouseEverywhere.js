
var mouseButton;
$(window).mousedown(function (e) { if (e.which == 2) { mouseButton = 2; } else { mouseButton = 0; } });

STSNavigate2 = function STSNavigate2(evt, url) {
    //debugger;
    if (typeof ieMouseEvent === 'undefined') {
        if (!evt) {
            if (mouseButton == 2) {
                window.open(url, '_blank');
            } else {
                STSNavigate(url);
            }
        } else {
            if (evt.which == 2) {
                window.open(url, '_blank');
            } else {
                STSNavigate(url);
            }
        }
    } else {
        if (ieMouseEvent == 2) {
            window.open(url, '_blank');
        } else {
            STSNavigate(url);
        }
    }
};
GoToPage = function GoToPage(url, fOnlyUseExistingSource) {
    var ch = url.indexOf("?") >= 0 ? "&" : "?";

    if ((GetUrlKeyValue("Source", true, url)).length == 0) {
        var srcUrl = fOnlyUseExistingSource ? GetUrlKeyValue("Source") : GetSource();

        if (srcUrl != null && srcUrl != "") {
            if (fOnlyUseExistingSource)
                srcUrl = escapeProperly(STSPageUrlValidation(srcUrl));
            if (url.length + srcUrl.length <= 1950) {
                url += ch + "Source=" + srcUrl;
            }
        }
    }
    STSNavigate2(event, url);
};
if (navigator.userAgent.toLowerCase().indexOf('msie') != -1) {
    var ieMouseEvent;
    var origWpClick;
    var origTrapMenuClick;
    setTimeout(function () {
        origWpClick = WpClick;
        WpClick = function (evt) {
            ieMouseEvent = evt.which;
            return origWpClick(evt);
        }
        origTrapMenuClick = TrapMenuClick;
        TrapMenuClick = function (e) {
            if (e.button == 4)
            { ieMouseEvent = 2; }
            return origTrapMenuClick(e);
        }
    }, 700);
} else {
    GoToLink = function GoToLink(elm) {
        var targetUrl = GetGotoLinkUrl(elm);

        if (targetUrl == null)
            return;
        var fNavigate = true;

        if (typeof window.top.SPUpdatePage !== 'undefined') {
            fNavigate = window.top.SPUpdatePage(targetUrl);
        }
        if (fNavigate) {
            if (elm.target === "_blank" || elm.onclick.arguments[0].which == 2)
                window.open(targetUrl, "_blank");
            else if (isPortalTemplatePage(targetUrl))
                window.top.location.href = STSPageUrlValidation(targetUrl);
            else
                window.location.href = STSPageUrlValidation(targetUrl);
        }
    };
}
