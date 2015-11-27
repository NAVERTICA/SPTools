(function () {
    if (!window.JSONschemas || !window.loadSchema) {
        console.log("missing global functions and objects - running in wrong context?");        
    }

    
    window.JSONschemas["ItemDeleting"] = function(contents, title, force) {
        if (loadSchema("ItemReceiver", contents, force)) {
            window.JSONschemas["ItemDeleting"] = window.JSONschemas["ItemReceiver"];
            return window.JSONschemas["ItemDeleting"](contents, "ItemDeleting", force);
        }
        return null;
    }    
})();
