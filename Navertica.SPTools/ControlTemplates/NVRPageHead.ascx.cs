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