
(function () {
    var CustomScriptContext = {};

    // You can provide templates for:
    // View, DisplayForm, EditForm and NewForm
    CustomScriptContext.Templates = {};
    CustomScriptContext.Templates.Fields = {
        "Note": {
            //"View": CustomScriptEdit,
            //"DisplayForm": CustomScriptEdit,
            "EditForm": CustomScriptEdit
        }
    };

    SPClientTemplates.TemplateManager.RegisterTemplateOverrides(CustomScriptContext);
})();

function CustomScriptEdit(ctx) {
    var formCtx = SPClientTemplates.Utility.GetFormContextForCurrentField(ctx);
    var n = formCtx.fieldName;
    var i = formCtx.fieldValue;
    var t = formCtx.fieldSchema.Title;

    var file = ctx.CurrentItem.FileLeafRef;
    var ext = file.split('.').pop().toLowerCase();
    /*console.log(ext);
	console.log(n);
	console.log(i);
	console.log(t); */

    var cdm = CreateCodeMirror(t, i, ext);

    formCtx.registerGetValueCallback(n, function () {
        var newValue = cdm.getValue();
        if (!newValue) { return ' '; }
        //formCtx.fieldValue = newValue;
        return newValue;
    });
    return "";
};

function CreateCodeMirror(name, inp, ext) {
    var mode = null;
    loadScript('/_layouts/15/SPTools/codemirror/lib/codemirror.js');

    switch (ext) {
        case "js":
            loadScript('/_layouts/15/SPTools/codemirror/mode/javascript/javascript.js');
            loadScript('/_layouts/15/SPTools/codemirror/addon/hint/javascript-hint.js');
            loadScript('/_layouts/15/SPTools/codemirror/addon/lint/lint.js');
            headAppendCSS('/_layouts/15/SPTools/codemirror/addon/lint/lint.css');
            loadScript('/_layouts/15/SPTools/codemirror/addon/lint/javascript-lint.js');
            mode = "javascript";
            break;
        case "json":
            loadScript('/_layouts/15/SPTools/codemirror/mode/javascript/javascript.js');
            // loadScript('/_layouts/15/SPTools/codemirror/addon/hint/javascript-hint.js');
            // loadScript('/_layouts/15/SPTools/codemirror/addon/lint/lint.js');
            // headAppendCSS('/_layouts/15/SPTools/codemirror/addon/lint/lint.css');
            // loadScript('/_layouts/15/SPTools/codemirror/addon/lint/json-lint.js');
            mode = "application/json";
            break;
        case "css":
            loadScript('/_layouts/15/SPTools/codemirror/mode/css/css.js');
            // loadScript('/_layouts/15/SPTools/codemirror/addon/hint/css-hint.js');
            // loadScript('/_layouts/15/SPTools/codemirror/addon/lint/lint.js');
            // headAppendCSS('/_layouts/15/SPTools/codemirror/addon/lint/lint.css');
            // loadScript('/_layouts/15/SPTools/codemirror/addon/lint/json-lint.js');
            mode = "text/css";
            break;
        case "py":
            loadScript('/_layouts/15/SPTools/codemirror/mode/python/python.js');
            // loadScript('/_layouts/15/SPTools/codemirror/addon/hint/python-hint.js');
            mode = "python";
            break;
        case "csharp":
            loadScript('/_layouts/15/SPTools/codemirror/mode/clike/clike.js');
            // loadScript('/_layouts/15/SPTools/codemirror/addon/hint/anyword-hint.js');
            mode = "text/x-csharp";
            break;
        case "html":
            loadScript('/_layouts/15/SPTools/codemirror/mode/xml/xml.js');
            // loadScript('/_layouts/15/SPTools/codemirror/addon/hint/html-hint.js');
            mode = "text/html";
            break;
        case "sql":
            loadScript('/_layouts/15/SPTools/codemirror/mode/plsql/plsql.js');
            // loadScript('/_layouts/15/SPTools/codemirror/addon/hint/sql-hint.js');
            mode = "text/x-plsql";
            break;
        case "xml":
            loadScript('/_layouts/15/SPTools/codemirror/mode/xml/xml.js');
            // loadScript('/_layouts/15/SPTools/codemirror/addon/hint/xml-hint.js');
            mode = "application/xml";
            break;
        default:
            mode = "text";
            break;
    }
    //default
    headAppendCSS('/_layouts/15/SPTools/codemirror/lib/codemirror.css');
    //headAppendCSS('/_layouts/15/SPTools/codemirror/theme/default.css');
    //show-hint addon
    loadScript('/_layouts/15/SPTools/codemirror/addon/hint/show-hint.js');
    headAppendCSS('/_layouts/15/SPTools/codemirror/addon/hint/show-hint.css');
    //fullscreen addon
    headAppendCSS('/_layouts/15/SPTools/codemirror/addon/display/fullscreen.css');
    loadScript('/_layouts/15/SPTools/codemirror/addon/display/fullscreen.js');
    //active-line addon
    loadScript('/_layouts/15/SPTools/codemirror/addon/selection/active-line.js');
    // search/match addon
    loadScript('/_layouts/15/SPTools/codemirror/addon/dialog/dialog.js');
    headAppendCSS('/_layouts/15/SPTools/codemirror/addon/dialog/dialog.css');
    loadScript('/_layouts/15/SPTools/codemirror/addon/search/search.js');
    loadScript('/_layouts/15/SPTools/codemirror/addon/search/match-highlighter.js');
    loadScript('/_layouts/15/SPTools/codemirror/addon/search/searchcursor.js');
    //search addon - old
    // loadScript('/_layouts/15/SPTools/codemirror/lib/util/dialog.js');
    // headAppendCSS('/_layouts/15/SPTools/codemirror/lib/util/dialog.css');
    // loadScript('/_layouts/15/SPTools/codemirror/lib/util/search.js');
    // loadScript('/_layouts/15/SPTools/codemirror/lib/util/searchcursor.js');

    cm = CodeMirror(getFormBody(name)[0], {
        mode: mode,
        gutter: true,
        fixedGutter: true,
        textWrapping: false,
        matchBrackets: true,
        lineNumbers: true,
        indentUnit: 4,
        tabMode: "shift"
    });
    cm.setValue(inp);
    return cm;
}