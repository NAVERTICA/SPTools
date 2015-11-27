using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using Microsoft.SharePoint;
using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.RepoService;

namespace Navertica.SharePoint.PageHead
{
    public class CSSHead : UserControl, IPageHead
    {
        public string Category()
        {
            return "CSS";
        }

        public object LoadContents(ConfigServiceClient cfg, RepoServiceClient repo)
        {
            if (cfg == null || repo == null) throw new ArgumentNullException();

            StringBuilder stylesResult = new StringBuilder();
            stylesResult.Append("<style>").AppendLine();

            var cfgStyles = GetConfigGuidList(cfg);

            foreach (Guid cfgId in cfgStyles)
            {
                PageHeadConfig c = (PageHeadConfig) cfg.GetBranch<PageHeadConfig>(cfgId, this.Category());

                if (c != null)
                {
                    // v SiteConfigu mohou byt jen jmena souboru z adresare dane aplikace v SiteScripts, 
                    // nikdy primo kod, ktery by se vkladal do stranky (mozna u css by to tak byt nemuselo)
                    foreach (string scriptFileName in c.StyleLinks)
                    {
                        if (string.IsNullOrWhiteSpace(scriptFileName)) continue;
                        string scriptName = scriptFileName.ToLowerInvariant().EndsWith(".css") ? scriptFileName : scriptFileName + ".css";
                        var scriptKvp = repo.Get(scriptName);

                        if (scriptKvp != null)
                        {
                            stylesResult.Append("/* " + scriptName + " start */\n");
                            stylesResult.Append(scriptKvp.StringUnicode);
                            stylesResult.Append("/* " + scriptName + " end */\n");
                        }
                    }
                }
            }

            stylesResult.Append("</style>");

            return stylesResult.ToString();
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