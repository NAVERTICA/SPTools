
(function () {
    var jsonContext = {};

    // You can provide templates for:
    // View, DisplayForm, EditForm and NewForm
    jsonContext.Templates = {};
    jsonContext.Templates.Fields = {
        "JSONField": {
            // "View": JSONFieldViewTemplate,
            // "DisplayForm": JSONFieldViewTemplate,
            "EditForm": JSONFieldEditTemplate
        }
    };

    SPClientTemplates.TemplateManager.RegisterTemplateOverrides(jsonContext);
})();


function JSONFieldEditTemplate(ctx) {
    var formCtx = SPClientTemplates.Utility.GetFormContextForCurrentField(ctx);
    var n = formCtx.fieldName;
    var i = formCtx.fieldValue;
    var t = formCtx.fieldSchema.Title;

    /* console.log(n);
	console.log(i);
	console.log(t); */

    var cdm = CreateCodeMirror(t, i);

    //console.log(cdm.getValue());
    formCtx.registerGetValueCallback(n, function () {
        var newValue = cdm.getValue();
        alert(newValue);
        if (!newValue) { return ' '; }
        //formCtx.fieldValue = newValue;
        return newValue;
    });
    return "";
};

function CreateCodeMirror(name, inp) {
    loadScript('/_layouts/15/SPTools/codemirror/lib/codemirror.js');
    loadScript('/_layouts/15/SPTools/codemirror/mode/javascript/javascript.js');
    headAppendCSS('/_layouts/15/SPTools/codemirror/lib/codemirror.css');
    //headAppendCSS('/_layouts/15/SPTools/codemirror/theme/default.css');

    cm = CodeMirror(getFormBody(name)[0], {
        mode: "application/json",
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