(function () {
    if (!window.JSONschemas || !window.loadSchema) {
        console.log("missing global functions and objects - running in wrong context?");        
    }

    
    window.JSONschemas["ItemAdded"] = function(contents, title, force) {
        if (loadSchema("ItemReceiver", contents, force)) {
            window.JSONschemas["ItemAdded"] = window.JSONschemas["ItemReceiver"];
            return window.JSONschemas["ItemAdded"](contents, "ItemAdded", force);
        }
        return null;
    }    
})();
