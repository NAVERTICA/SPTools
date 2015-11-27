/*  Copyright (C) 2015 NAVERTICA a.s. http://www.navertica.com 

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.  */
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using Microsoft.SharePoint;
using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.RepoService;
using Newtonsoft.Json.Linq;
using  Navertica.SharePoint.PageHead;

namespace Navertica.SharePoint.PageHead
{
    public class TranslationsHead : UserControl, Navertica.SharePoint.PageHead.IPageHead
    {
        public string Category()
        {
            return "Translations";
        }

        public object LoadContents(ConfigServiceClient cfg, RepoServiceClient repo)
        {
            if (cfg == null) throw new ArgumentNullException();

            StringBuilder translationsResult = new StringBuilder();
            translationsResult.Append("<script type='text/javascript'>").AppendLine();
            //ribbonResult.Append("make sure createButtons function is loaded")
            translationsResult.Append("if(!typeof(NVRHlasky)=='object' && $.isEmptyObject(NVRHlasky)){var NVRHlasky = {};}").AppendLine();//if Hlasky != emptyObject => Expand object instead
            var cfgTranslations = GetConfigGuidList(cfg);
            foreach (Guid cfgId in cfgTranslations)
            {
                string c = cfg.GetBranchJson(cfgId, this.Category());
                if (c != null)
                {
                    JObject jc = JObject.Parse(c);
                    if (jc["error"] != null)
                    {
                        translationsResult.Append("console.error('problem loading translations config: " + jc["error"] + "');");
                        continue;
                    }

                    JToken jTranslations = jc["Hlasky"];
                    //translations = (jc["Hlasky"] ?? "").ToString();

                    foreach (JProperty translation in jTranslations)
                    {
                        var key = translation.Name;
                        var value = translation.Value;
                        translationsResult.Append("NVRHlasky = $.extend({}, NVRHlasky, {\"" + key + "\": " + value + "});").AppendLine();
                    }
                }
            }

            translationsResult.Append("</script>");
            return translationsResult.ToString();
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