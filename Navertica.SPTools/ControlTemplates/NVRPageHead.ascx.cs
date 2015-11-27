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
using System.Web.UI;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.SharePoint.Common.ServiceLocation;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.Extensions;
using Navertica.SharePoint.Logging;
using Navertica.SharePoint.RepoService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Navertica.SharePoint.PageHead
{
    public class PageHeadConfig : ConfigBranch
    {
        public JArray ScriptLinks;
        public JArray LibLinks;
        public JArray StyleLinks;

        public override string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override object Initialize()
        {
            if (!string.IsNullOrEmpty(( Json ?? "" ).Trim()))
            {
                try
                {
                    dynamic tmp = JsonConvert.DeserializeObject(Json);
                    ScriptLinks = tmp.ScriptLinks;
                    LibLinks = tmp.LibLinks;
                    StyleLinks = tmp.StyleLinks;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return this;
        }
    }

    /// <summary>
    /// Adding javascripts and styles on pages 
    /// </summary>
    public partial class PageHead : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (SPContext.Current == null) return;

            var log = NaverticaLog.GetInstance();

            using (new SPMonitoredScope("PageHead"))
            {
                try
                {
                    SPSite site = SPContext.Current.Site;
                    ConfigServiceClient cfg = new ConfigServiceClient(site);
                    RepoServiceClient repo = new RepoServiceClient(site);

                    if (this.Request.QueryString["Reset"].ToBool())
                    {
                        cfg.Reset(site.Url);
                        repo.Reset();
                        PluginHost.Reset();
                    }

                    IServiceLocator serviceLocator = SharePointServiceLocator.GetCurrent(site);
                    IEnumerable<IPageHead> executeInterfaces = serviceLocator.GetAllInstances<IPageHead>();

                    foreach (IPageHead pageInterface in executeInterfaces)
                    {
                        ModulesLiteral.Text += "<!-- NVR PageHead " + pageInterface.Category() + " start -->\n";
                        using (new SPMonitoredScope(pageInterface.Category()))
                        {
                            ModulesLiteral.Text += (string)pageInterface.LoadContents(cfg, repo);
                        }

                        ModulesLiteral.Text += "<!-- NVR PageHead " + pageInterface.Category() + " END -->\n";
                    }
                }
                catch (Exception ex)
                {
                    log.LogException(ex);
                }
            }
        }
    }
}