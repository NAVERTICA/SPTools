using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using Microsoft.SharePoint;
using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.RepoService;
using Newtonsoft.Json.Linq;
using Navertica.SharePoint.PageHead;

namespace Navertica.SharePoint.PageHead
{
    public class RibbonHead : UserControl, Navertica.SharePoint.PageHead.IPageHead
    {
        public string Category()
        {
            return "Ribbon";
        }

        public object LoadContents(ConfigServiceClient cfg, RepoServiceClient repo)
        {
            if (cfg == null) throw new ArgumentNullException();

            StringBuilder ribbonResult = new StringBuilder();
            ribbonResult.Append("<script type='text/javascript'>").AppendLine();
            //ribbonResult.Append("make sure createButtons function is loaded")
            ribbonResult.Append("var controlProperties = [];").AppendLine();
            var cfgRibbons = GetConfigGuidList(cfg);
            string hideElements = "";
            foreach (Guid cfgId in cfgRibbons)
            {
                string c = cfg.GetBranchJson(cfgId, this.Category());
                if (c != null)
                {                    
                    JObject jc = JObject.Parse(c);
                    if (jc["error"] != null)
                    {
                        ribbonResult.Append("console.error('problem loading ribbon config: " + jc["error"] + "');");
                        continue;
                    }

                    //List<string> jControlProperties = jc["controlProperties"].ToObject<List<string>>();
                    JToken jControlProperties = jc["controlProperties"];
                    hideElements = (jc["hideSelector"] ?? "").ToString();

                    foreach (JObject controlProperty in jControlProperties)
                    {
                        string json = controlProperty.ToString();
                        ribbonResult.Append("controlProperties.push(");
                        ribbonResult.Append(json);
                        ribbonResult.Append(");").AppendLine();
                    }
                }
            }
            ribbonResult.Append("EnsureScriptFunc('createButtons.js', 'createButtons', function() {" +
                                    "EnsureScriptFunc('ribbon', 'SP.Ribbon.PageManager', function() {" +
                                        "if(controlProperties.length!=0)" +
                                            "createButtons(controlProperties);" +
                                            (!string.IsNullOrEmpty(hideElements) ? ("$('" + hideElements.Replace("'", "\\'") + "').hide();") : "") +
                                    "});" +
                                "});").AppendLine();
            ribbonResult.Append("</script>");
            return ribbonResult.ToString();
        }

        public ConfigGuidList GetConfigGuidList(ConfigServiceClient cfg)
        {
            var filters = new Dictionary<ConfigFilterType, string>
            {
                { ConfigFilterType.App, this.Category() },
                { ConfigFilterType.Url, this.Context.Request.RawUrl }
            };

            if (SPContext.Current.List != null)
            {
                filters.Add(ConfigFilterType.ListType, SPContext.Current.List.BaseType.ToString());
            }

            if (SPContext.Current.ListItem != null)
            {
                filters.Add(ConfigFilterType.ContentType, SPContext.Current.ListItem.ContentTypeId.ToString());
            }

            return cfg.Filter(SPContext.Current.Web, filters);
        }
    }
}