(function () {
    if (!window.JSONschemas || !window.loadSchema) {
        console.log("missing global functions and objects - running in wrong context?");        
    }

    
    window.JSONschemas["ItemUpdating"] = function (contents, title, force) {
        if (loadSchema("ItemReceiver", contents, force)) {
            window.JSONschemas["ItemUpdating"] = window.JSONschemas["ItemReceiver"];
            return window.JSONschemas["ItemUpdating"](contents, "ItemUpdating", force);
        }
        return null;
    }    
})();
