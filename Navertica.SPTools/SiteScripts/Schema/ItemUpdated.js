(function () {
    if (!window.JSONschemas || !window.loadSchema) {
        console.log("missing global functions and objects - running in wrong context?");        
    }

    
    window.JSONschemas["ItemUpdated"] = function(contents, title, force) {
        if (loadSchema("ItemReceiver", contents, force)) {
            window.JSONschemas["ItemUpdated"] = window.JSONschemas["ItemReceiver"];
            return window.JSONschemas["ItemUpdated"](contents, "ItemUpdated", force);
        }
        return null;
    }    
})();
