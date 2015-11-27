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
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using Navertica.SharePoint.Extensions;
using Navertica.SharePoint.Interfaces;
using Navertica.SharePoint.Logging;
using Navertica.SharePoint.RepoService;

namespace Navertica.SharePoint.Pages
{
    public partial class Execute : LayoutsPageBase
    {
        private void LogError(ILogging log, string message)
        {
            log.LogError(message);

            Response.Write(message);
            Response.StatusCode = 404;
            Response.StatusDescription = message;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            using (new SPMonitoredScope("Navertica.SharePoint.Pages. Execute"))
            {
                SPSite site = SPContext.Current.Site;
                SPWeb web = null;
                bool dispose = false;

                RepoServiceClient scriptrepo = new RepoServiceClient(site);
                ILogging log = NaverticaLog.GetInstance();

                //Pokud neni explicitne uveden web nacitame contextovy web. Pri volani na konkretnim webu muzeme pouzit (pro optimalizaci vykonu)  /cfp/_layouts/15/Execute.aspx?List=&item=&atd.....
                try
                {
                    if (!string.IsNullOrEmpty(Request["Web"]))
                    {
                        web = site.OpenW(( Request["Web"] ?? "/" ), true);
                        dispose = true;
                    }
                    else
                    {
                        web = SPContext.Current.Web;
                    }
                }
                catch (Exception exc)
                {
                    var msg = "Could not get Web " + exc;
                    LogError(log, msg);
                    return;
                }

                web.RunWithAllowUnsafeUpdates(() =>
                {
                    #region Scope data

                    DictionaryNVR scopeData = new DictionaryNVR
                    {
                        { "Request", Request },
                        { "Response", Response },
                        { "site", site },
                        { "Server", Server },
                        { "web", web },
                        { "Page", Page },
                        { "Cache", Cache }
                    };

                    if (!string.IsNullOrEmpty(Request["List"]))
                    {
                        try
                        {
                            SPList list = web.OpenList(Request["List"], true);
                            scopeData["list"] = list;

                            if (!string.IsNullOrEmpty(Request["Item"]))
                            {
                                try
                                {
                                    int id = int.Parse(Request["Item"]);
                                    SPListItem item = list.GetItemById(id);
                                    scopeData["item"] = item;
                                }
                                catch (Exception)
                                {
                                    LogError(log, "Could not get item with '" + Request["Item"] + "'");
                                    return;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            LogError(log, "Could not get list '" + Request["List"] + "' at web " + web.Url);
                            return;
                        }
                    }

                    #endregion

                    if (!string.IsNullOrWhiteSpace(Request["PluginName"]))
                    {
                        ExecutePluginName(scopeData, site, log);
                    }
                    else if (!string.IsNullOrWhiteSpace(Request["ScriptPath"]))
                    {
                        // TODO - tuto vetev nejspis nebudeme vubec potrebovat, delat vsechno pres pluginy bude bezpecnejsi
                        ExecuteScriptPath(scriptrepo, scopeData, web, log);
                    }
                    else
                    {
                        LogError(log, "Execute.aspx needs PluginName or ScriptPath");
                    }
                });

                if (dispose) web.Dispose();
            }
        }

        private void ExecuteScriptPath(RepoServiceClient scriptrepo, DictionaryNVR scopeData, SPWeb web, ILogging log)
        {
            try
            {
                var context = scriptrepo.Execute(Request["ScriptPath"], new ScriptContext(scopeData));

                if (context == null)
                {
                    Response.Write("Invalid path/file name for parameter Script");
                    Response.StatusCode = 404;
                    Response.StatusDescription = "Invalid path/file name for parameter Script";
                    return;
                }

                web.AllowUnsafeUpdates = false;
            }
            catch (Exception exc)
            {
                log.LogException("Exception in Execute.aspx, Script " + Request["ScriptPath"], exc);
                Response.Write("Exception in script\n" + exc);
                Response.StatusCode = 500;
                Response.StatusDescription = "Exception in script";
                return;
            }
        }

        private void ExecutePluginName(DictionaryNVR scopeData, SPSite site, ILogging log)
        {
            try
            {
                // TODO - konfigurace by se dala filtrovat podle jmena pluginu + lokace stranky, ze ktere se Execute vola
                // ted to je na samotnem pluginu, aby si nacetl, co potrebuje

                PluginHost.Init(site);

                string pluginName = Request["PluginName"];

                IPlugin plugin = ExecutePagePluginHost.Get(site, pluginName);
                if (plugin == null)
                {
                    log.LogError(string.Format("ExecuteScriptPlugin named {0} was not found in loaded plugins, skipping execution", pluginName));
                    return;
                }

                try
                {
                    using (new SPMonitoredScope("Executing ExecuteScriptPlugin " + pluginName))
                    {
                        var res = plugin.Execute(null, scopeData);
                        log.LogInfo("Executed " + pluginName + " with result:\n", res ?? "NULL");
                    }
                }
                catch (Exception e)
                {
                    log.LogError(string.Format("ExecuteScriptPlugin named {0} has thrown {1}", pluginName, e),
                        e.InnerException, e.StackTrace, e.Source);
                }
            }
            catch (Exception exc)
            {
                log.LogException("Exception in Execute.aspx, Script " + Request["ScriptPath"], exc);
                Response.Write("Exception in script\n" + exc);
                Response.StatusCode = 500;
                Response.StatusDescription = "Exception in script";
                return;
            }
        }
    }
}