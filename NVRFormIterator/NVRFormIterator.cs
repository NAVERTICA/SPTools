using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.Extensions;
using Navertica.SharePoint.Interfaces;
using Navertica.SharePoint.Logging;
using Navertica.SharePoint.RepoService;

namespace Navertica.SharePoint.WebControls
{
    public class FormIteratorPluginConfig : ConfigBranch
    {
        public class PluginsFormIterator
        {
            public string Prefill;
            public string Nonedit;
            public string Exclude;
        }

        public class FormTemplatesFormIterator
        {
            public string NewForm;
            public string EditForm;
            public string DisplayForm;
        }

        public class TabsFormIterator
        {
            public string LookupField;
            public string Url;
            public string Selector;
            public int Order;
            public List<LanguageTitle> Title;
        }

        public PluginsFormIterator Plugins;
        public FormTemplatesFormIterator FormTemplates;
        public List<TabsFormIterator> Tabs;
    }

    /// <summary>    
    /// Used for every standard NewForm, EditForm and DisplayForm
    /// Evaluates what should be displayed for the current user, what should be prefilled or disabled
    /// </summary>
    public class FormIterator2013 : ListFieldIterator
    {
        private Literal formIteratorLiteral;

        private DictionaryNVR prefillPluginData;
        private DictionaryNVR modesPluginData;
        private DictionaryNVR excludePluginData;

        private Dictionary<string, string> Tabs = new Dictionary<string, string>();

        private string customFormScript =
            "runOnEachPostback.push(function(){" +
            "	// Hide standard form" +
            "	$(\".ms-formtable\").css({ \"display\": \"none\" });" +
            "	$('table#NVR_CustomForm span').each(function(){" +
            "	if(typeof($(this).attr(\"field-internalName\"))!='undefined'){" +
            "		var internalName = $(this).attr(\"field-internalName\");" +
            "		if(this.className == \"NVR_customFieldData\"){	$(getFormBody(internalName)).parent().find(\".ms-formbody\").contents().appendTo($(this));	}" +
            "		if(this.className == \"NVR_customFieldName\"){	$(getFormBody(internalName)).parent().find(\".ms-formlabel\").contents().appendTo($(this));	}" +
            "	}" +
            "});" +
            "	var customFormHtml = $(\"#NVR_CustomForm\").html();" +
            "	$(\"#DeltaPlaceHolderMain\").prepend(customFormHtml);" +
            "});";

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            SPContext context = SPContext.Current;

            using (new SPMonitoredScope("FormIterator2013 - CreateChildControls"))
            {
                ILogging log = NaverticaLog.GetInstance();

                if (context == null)
                {
                    log.LogError("SPContext in formiterator is null");
                    return;
                }

                RepoServiceClient repo = new RepoServiceClient(context.Site);
                ConfigServiceClient configurations = new ConfigServiceClient(context.Site);

                #region Hide ContentType in EditForm

                bool? showInEdit = context.List.GetFieldByInternalName("ContentType").ShowInEditForm;
                if (showInEdit.HasValue && showInEdit.ToBool() == false)
                {
                    formIteratorLiteral = (Literal) FindControlRecursive(Page, "FormIteratorLiteral");
                    formIteratorLiteral.Text = "<!-- Hide ContentType Field -->" +
                                               "<script type='text/javascript'>runOnEachPostback.push(function() { $('[id*=\"ContentTypeChoice\"]').parent().parent().css(\"display\", \"none\"); });</script>";
                }

                #endregion

                SPListItem item = context.ListItem;

                ConfigGuidList cfgs = configurations.Filter(context.Web, new Dictionary<ConfigFilterType, string>
                {
                    { ConfigFilterType.App, "FormIterator" },
                    { ConfigFilterType.ContentType, item.ContentTypeId.ToString() },
                    { ConfigFilterType.ListType, item.ParentList.BaseTemplate.ToString() },
                    { ConfigFilterType.Url, Page.Request.Url.AbsolutePath }
                });

                if (cfgs.Count != 0)
                {
                    PluginHost.Init(context.Site);

                    SPControlMode formMode = context.FormContext.FormMode;

                    // if several configurations are found, the one with lowest Order will be used
                    ConfigBranch cfg = configurations.GetBranch<ConfigBranch>(cfgs[0], "FormIterator");
                    FormIteratorPluginConfig config = ConfigBranch.GetInstance<FormIteratorPluginConfig>(cfg);

                    #region Custom Forms

                    var formTemplates = config.FormTemplates;

                    if (formTemplates != null && ( formTemplates.NewForm != null || formTemplates.EditForm != null || formTemplates.DisplayForm != null ))
                    {
                        bool customForm = ( this.Page.Request.QueryString["CustomForm"] ?? "1" ).ToBool();

                        try
                        {
                            if (customForm)
                            {
                                formIteratorLiteral.Text += customFormScript;

                                if (formTemplates.NewForm != null && ( formMode == SPControlMode.New ))
                                {
                                    formIteratorLiteral.Text = repo.Get(formTemplates.NewForm).StringUnicode;
                                }

                                if (formTemplates.EditForm != null && ( formMode == SPControlMode.Edit ))
                                {
                                    formIteratorLiteral.Text = repo.Get(formTemplates.EditForm).StringUnicode;
                                }

                                if (formTemplates.DisplayForm != null && ( formMode == SPControlMode.Display ))
                                {
                                    formIteratorLiteral.Text = repo.Get(formTemplates.DisplayForm).StringUnicode;
                                }
                            }
                        }
                        catch (Exception exc)
                        {
                            log.LogException(exc);
                        }
                    }

                    #endregion

                    #region PLUGIN LOADING DATA

                    var plugins = config.Plugins;
                    if (plugins != null)
                    {
                        if (!String.IsNullOrEmpty(plugins.Exclude))
                        {
                            string pluginName = plugins.Exclude;
                            IPlugin plugin = ExecutePagePluginHost.Get(context.Site, pluginName);

                            if (plugin == null)
                            {
                                log.LogError(string.Format("FormIterator2013 ExcludePlugin named {0} was not found in loaded plugins, skipping execution", pluginName));
                            }
                            else
                            {
                                try
                                {
                                    excludePluginData = (DictionaryNVR) plugin.Execute(null, item);
                                }
                                catch (Exception) {}
                            }
                        }

                        if (!String.IsNullOrEmpty(plugins.Prefill) && ( formMode == SPControlMode.Edit || formMode == SPControlMode.New ))
                        {
                            string pluginName = plugins.Prefill;
                            IPlugin plugin = ExecutePagePluginHost.Get(context.Site, pluginName);

                            if (plugin == null)
                            {
                                log.LogError(string.Format("FormIterator2013 PrefillPlugin named {0} was not found in loaded plugins, skipping execution", pluginName));
                            }
                            else
                            {
                                try
                                {
                                    prefillPluginData = (DictionaryNVR) plugin.Execute(null, item);
                                }
                                catch (Exception) {}
                            }
                        }

                        if (!String.IsNullOrEmpty(plugins.Nonedit) && ( formMode == SPControlMode.Edit ))
                        {
                            string pluginName = plugins.Nonedit;
                            IPlugin plugin = ExecutePagePluginHost.Get(context.Site, pluginName);

                            if (plugin == null)
                            {
                                log.LogError(string.Format("FormIterator2013 NoneditPlugin named {0} was not found in loaded plugins, skipping execution", pluginName));
                            }
                            else
                            {
                                try
                                {
                                    modesPluginData = (DictionaryNVR) plugin.Execute(null, item);
                                }
                                catch (Exception) {}
                            }
                        }
                    }

                    #endregion

                    #region Tabs

                    if (config.Tabs != null)
                    {
                        string cultureName = CultureInfo.CurrentUICulture.Name;

                        foreach (var t in config.Tabs.OrderBy(t => t.Order))
                        {
                            var tab = t;
                            string url = tab.Url;
                            string tabTitle = null;

                            if (!string.IsNullOrEmpty(tab.LookupField)) // load a lookup field
                            {
                                try
                                {
                                    SPFieldLookup lookupField = (SPFieldLookup) item.ParentList.OpenField(tab.LookupField);
                                    WebListId wli = new WebListId(lookupField.LookupWebId.ToString(), lookupField.LookupList);

                                    wli.ProcessList(context.Site, delegate(SPList lookupList)
                                    {
                                        if (!url.EndsWith("?")) url += "?";

                                        if (lookupField.AllowMultipleValues)
                                        {
                                            SPFieldLookupValueCollection values = (SPFieldLookupValueCollection) context.ListItem.Get(lookupField.InternalName);
                                            if (values.Count > 0)
                                            {
                                                url += "FilterName=ID&FilterMultiValue=" + values.Select(ids => ids.LookupId).JoinStrings(";");
                                            }
                                        }
                                        else
                                        {
                                            SPFieldLookupValue value = (SPFieldLookupValue) context.ListItem.Get(lookupField.InternalName);
                                            if (item != null)
                                            {
                                                url += "FilterField1=ID&FilterValue1=" + value.LookupId;
                                            }
                                        }

                                        tabTitle = lookupField.Title;

                                        return null;
                                    });

                                }
                                catch (Exception exc)
                                {
                                    log.LogException(exc);
                                }
                            }

                            if (tab.Title != null && tab.Title.Count > 0)
                            {
                                var title = tab.Title.SingleOrDefault(l => l.Language == cultureName);
                                if (title != null)
                                {
                                    tabTitle = title.Title;
                                }
                            }

                            if (tabTitle == null)
                            {
                                tabTitle = "TAB - " + tab.Order;
                            }

                            Tabs.Add(tabTitle, url);
                        }
                    }

                    #endregion
                }
            }

            this.Controls.Clear();

            if (this.ControlTemplate == null) throw new ArgumentException("Could not find ListFieldIterator control template.");

            var webBag = SPContext.Current.Site.RootWeb.Properties;
            var listBag = SPContext.Current.List.RootFolder.Properties;
            var siteFields = webBag.ContainsKey("NonEditable") ? webBag["NonEditable"].Split(',') : new string[0];
            var listFields = listBag.ContainsKey("NonEditable") ? ( listBag["NonEditable"] ?? "" ).ToString().Split(',') : new string[0];

            for (int i = 0; i < Fields.Count; i++)
            {
                SPField field = Fields[i];

                // TODO doesn't work well
                bool enabledInForm = !this.IsFieldExcluded(field);
                if (enabledInForm)
                {
                    #region Prefill data from plugin

                    PrefillForm(field);

                    #endregion

                    TemplateContainer fieldContainer = new TemplateContainer();

                    SPControlMode mode = ControlMode;

                    #region set DisplayMode in EditForm

                    if (SPContext.Current.FormContext.FormMode == SPControlMode.Edit)
                    {
                        if (siteFields.Contains(field.InternalName))
                        {
                            if (listFields.Contains(field.InternalName)) mode = SPControlMode.Edit; 
                            else mode = SPControlMode.Display;
                        }
                        else
                        {
                            if (listFields.Contains(field.InternalName)) mode = SPControlMode.Display;
                        }

                        if (modesPluginData != null && modesPluginData.ContainsKey(field.InternalName))
                        {
                            mode = ( modesPluginData[field.InternalName].GetType() == typeof (SPControlMode) ) ? (SPControlMode) modesPluginData[field.InternalName] : SPControlMode.Edit;
                        }
                    }

                    #endregion

                    const BindingFlags flags = BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic;
                    var controlMode = fieldContainer.GetType().GetProperty("ControlMode", flags);
                    var fieldName = fieldContainer.GetType().GetProperty("FieldName", flags);
                    controlMode.SetValue(fieldContainer, mode, null);
                    fieldName.SetValue(fieldContainer, field.InternalName, null);

                    this.Controls.Add(fieldContainer);
                    this.ControlTemplate.InstantiateIn(fieldContainer);
                }
            }

            if (Tabs.Count != 0)
            {
                formIteratorLiteral.Text += BuildTabs();
                formIteratorLiteral.Text += "<script type='text/javascript'>enableJQueryUI();</script>" +
                                            "<script type='text/javascript'>" +
                                            "runOnEachPostback.push(function() {" +
                                            "$( \"#NVR_tabs\").tabs();" +
                                            "$($('#DeltaPlaceHolderMain')[0]).append($('#NVR_tabs')[0]);" +
                                            "$('#listFormToolBarTop').parent().parent()[0].style.height = 'auto';" + //SP-form css fix
                                            "$('.ui-helper-clearfix').css('max-height','30px');" + //jQ-UI css fix
                                            "});" +
                                            "</script>";
            }
        }

        private string BuildTabs()
        {
            StringBuilder sb = new StringBuilder();
            int counter = 0;

            //sb.Append("<div style=\"display:none\">");
            sb.Append("<div id=\"NVR_tabs\">");
            sb.Append("<ul>");
            foreach (KeyValuePair<string, string> kvp in Tabs)
            {
                counter++;
                sb.Append("<li><a href=\"#tabs" + counter + "\">" + kvp.Key + "</a></li>");
            }
            sb.Append("</ul>");

            counter = 0;
            foreach (KeyValuePair<string, string> kvp in Tabs)
            {
                counter++;
                sb.Append("<div id=\"tabs" + counter + "\"><iframe src=\"" + kvp.Value + "&IsDlg=1" + "\"  frameborder=\"0\" border=\"0\" cellspacing=\"0\" style=\"border-style: none;width: 100%; height: 100%; min-height: 600px;\"></iframe></div>");
            }

            //sb.Append("</div>");
            sb.Append("</div>");

            return sb.ToString();
        }

        protected override bool IsFieldExcluded(SPField field)
        {
            using (new SPMonitoredScope("FormIterator - IsFieldExcluded -" + field.InternalName))
            {
                bool excluded = base.IsFieldExcluded(field);

                if (excludePluginData != null && excludePluginData.ContainsKey(field.InternalName)) excluded = ( excludePluginData[field.InternalName] ?? false ).ToBool();

                return excluded;
            }
        }

        /// <summary>
        /// Data in URL has higher priority than what was loaded through a plugin
        /// </summary>
        /// <param name="field"></param>
        private void PrefillForm(SPField field)
        {
            BaseFieldControl ctrl = field.FieldRenderingControl;

            string value = Page.Request.QueryString[field.InternalName];

            if (prefillPluginData != null && prefillPluginData.ContainsKey(field.InternalName))
            {
                value = prefillPluginData[field.InternalName].ToString();
            }
            if (!string.IsNullOrEmpty(Page.Request.QueryString[field.InternalName]))
            {
                value = Page.Request.QueryString[field.InternalName];
            }

            if (!string.IsNullOrEmpty(value))
            {
                object result = null;
                string defaultValue = null;

                try
                {
                    switch (field.GetType().Name)
                    {
                            #region PREFILL BY JS

                        case "SPFieldUser":
                        {
                            List<SPPrincipal> principals = field.ParentList.ParentWeb.GetSPPrincipals(value.Split(','));
                            if (principals != null && principals.Count > 0)
                            {
                                StringBuilder scriptsResult = new StringBuilder();
                                scriptsResult.Append("<script type='text/javascript'>").AppendLine();
                                scriptsResult.Append("/* Prefill field " + field.InternalName + " with value '" + value + "' */\n");
                                scriptsResult.Append("runOnEachPostback.push(function() {NVR.e_n_d_form.putValueInInput('" + field.InternalName + "', '" + (principals.GetLogins().JoinStrings(",").Replace("\\", "\\\\")) + "');})\n");
                                scriptsResult.Append("</script>").AppendLine();
                                formIteratorLiteral.Text += scriptsResult.ToString();
                            }
                            break;
                        }
                        case "SPFieldLookup":
                        {
                            SPFieldLookupValueCollection lvc = new SPFieldLookupValueCollection();

                            if (value.Contains(";#"))
                            {
                                lvc = new SPFieldLookupValueCollection(value);
                            }
                            else
                            {
                                string[] ids = value.Split(',');
                                lvc.AddRange(ids.Select(id => new SPFieldLookupValue(id)));
                            }

                            if (lvc.Count > 0)
                            {
                                StringBuilder scriptsResult = new StringBuilder();
                                scriptsResult.Append("<script type='text/javascript'>").AppendLine();
                                scriptsResult.Append("/* Prefill field " + field.InternalName + " with value " + lvc + "*/\n");

                                foreach (int lookupId  in lvc.Select(lk => lk.LookupId))
                                {
                                    scriptsResult.Append("runOnEachPostback.push(function() {NVR.e_n_d_form.putValueInInput('" + field.InternalName + "', '" + lookupId + "');})\n");
                                }

                                scriptsResult.Append("</script>").AppendLine();
                                formIteratorLiteral.Text += scriptsResult.ToString();
                            }

                            break;
                        }

                            #endregion

                        case "SPFieldBoolean":
                        {
                            result = value.ToBool();
                            defaultValue = value.ToBool() ? "1" : "0";

                            break;
                        }
                        case "SPFieldChoice":
                        {
                            SPFieldChoice choice = (SPFieldChoice) field;
                            if (choice.Choices.Contains(value)) // by value
                            {
                                result = value;
                                field.DefaultValue = result.ToString();
                            }
                            else // by index
                            {
                                try
                                {
                                    int index = int.Parse(value);
                                    result = choice.Choices[index];
                                    field.DefaultValue = result.ToString();
                                }
                                catch {}
                            }

                            break;
                        }
                        case "SPFieldDateTime":
                        {
                            foreach (SPLanguage l in field.ParentList.ParentWeb.RegionalSettings.InstalledLanguages)
                            {
                                try
                                {
                                    CultureInfo info = new CultureInfo(l.LCID);
                                    DateTime date = DateTime.Parse(value, info);
                                    result = date;
                                    defaultValue = date.ToStringISO(info);
                                    break;
                                }
                                catch {}
                            }
                            break;
                        }
                        case "SPFieldUrl":
                        {
                            string[] urlParts = value.Split(',');
                            SPFieldUrlValue url = new SPFieldUrlValue();
                            url.Url = urlParts[0];
                            url.Description = urlParts.Length > 1 ? urlParts[1] : urlParts[0];

                            result = url;
                            defaultValue = url.ToString();
                            break;
                        }
                        case "SPFieldCalculated": 
                        case "SPFieldMultiLineText":
                        case "SPFieldNumber":
                        case "SPFieldText":
                        default:
                        {
                            result = value;
                            defaultValue = result.ToString();
                            break;
                        }
                    }

                    ctrl.ListItemFieldValue = result;

                    if (SPContext.Current.FormContext.FormMode == SPControlMode.New && defaultValue != null)
                    {
                        field.DefaultValue = defaultValue;
                    }
                }
                catch (Exception) {}
            }
        }

        private Control FindControlRecursive(Control root, string id)
        {
            if (root.ID == id) return root;

            foreach (Control c in root.Controls)
            {
                Control t = FindControlRecursive(c, id);
                if (t != null) return t;
            }
            return null;
        }
    }
}

/*
 // example config
 {
    "FormIterator": {
        "Plugins": {
            "Prefill": "TEST",
            "Nonedit": "TEST",
            "Exclude": "TEST"
        },
        "FormTemplates": {
            "NewForm": "/NVRFormTemplates/NVRTestTemplate.html",
            "EditForm": "/NVRFormTemplates/NVRTestTemplate.html",
            "DisplayForm": "/NVRFormTemplates/NVRTestTemplate.html"
        },
        "Tabs": [
            {
                "LookupField": "Lookup",
                "Url": "http://portal13devel/Lists/Contacts/AllItems.aspx",
                "Selector": "asasfasf",
                "Order": 100,
                "Title": {
                    "1029": "TESTTABCZ",
                    "1033": "TESTTABEN"
                }
            },
            {
                "LookupField": "MultiLookup",
                "Url": "http://portal13devel/Lists/Contacts/AllItems.aspx",
                "Selector": "asasfasf",
                "Order": 10,
                "Title": {
                    "1029": "TESTTABCZ2",
                    "1033": "TESTTABEN2"
                }
            }
        ]
    }
}
*/