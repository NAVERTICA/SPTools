using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using Microsoft.SharePoint;
using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.RepoService;
using Navertica.SharePoint.PageHead;

namespace Navertica.SharePoint.PageHead
{
    public class JavaScriptHead : UserControl, Navertica.SharePoint.PageHead.IPageHead
    {
        public string Category()
        {
            return "JavaScripts";
        }

        public object LoadContents(ConfigServiceClient cfg, RepoServiceClient repo)
        {
            if (cfg == null || repo == null) throw new ArgumentNullException();
            StringBuilder scriptsResult = new StringBuilder();

            var cfgScripts = GetConfigGuidList(cfg);

            foreach (Guid cfgId in cfgScripts)
            {
                PageHeadConfig c = (PageHeadConfig)cfg.GetBranch<PageHeadConfig>(cfgId, this.Category());
                if (c != null)
                {
                    string libFilePaths = "";
                    foreach (string scriptFileName in c.LibLinks)
                    {
                        if (string.IsNullOrWhiteSpace(scriptFileName)) continue;
                        string scriptName = scriptFileName.ToLowerInvariant().EndsWith(".js")
                            ? scriptFileName
                            : scriptFileName + ".js";
                        libFilePaths += scriptName + ";";
                    }
                    if (!string.IsNullOrWhiteSpace(libFilePaths)) 
                        scriptsResult.Append("<script type='text/javascript' src='/_layouts/Navertica.SPTools/GetScript.aspx?FilePaths=").Append(libFilePaths).Append("'></script>").AppendLine();

                    // v SiteConfigu mohou byt jen jmena souboru z adresare dane aplikace v SiteScripts, 
                    // nikdy primo kod, ktery by se vkladal do stranky (mozna u javascriptu by to tak byt nemuselo)
                    foreach (string scriptFileName in c.ScriptLinks)
                    {
                        if (string.IsNullOrWhiteSpace(scriptFileName)) continue;
                        /*string scriptName = scriptFileName.ToLowerInvariant().EndsWith(".js")
                            ? scriptFileName
                            : scriptFileName + ".js";*/

                        string scriptBody = repo.GetJavaScript(scriptFileName);
                        scriptsResult.Append(scriptBody);
                    }
                }
            }

            return scriptsResult.ToString();
        }

        public ConfigGuidList GetConfigGuidList(ConfigServiceClient cfg)
        {
            var filters = new Dictionary<ConfigFilterType, string>
            {
                { ConfigFilterType.App, Category() },
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