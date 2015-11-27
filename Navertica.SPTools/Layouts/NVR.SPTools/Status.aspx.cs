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
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.SharePoint.Common.ServiceLocation;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.Extensions;
using Navertica.SharePoint.Infrastructure;
using Navertica.SharePoint.Interfaces;
using Navertica.SharePoint.PageHead;
using Navertica.SharePoint.RepoService;

namespace Navertica.SharePoint.SPTools
{
    public class Status : LayoutsPageBase
    {
        protected Literal Content;
        protected Table ContentTable;

        private void PrintHeader(string title)
        {
            TableRow head = new TableRow();
            TableCell headCell = new TableCell { Text = "<h2> " + title + " </h2>", ColumnSpan = 2 };
            head.Cells.Add(headCell);
            ContentTable.Rows.Add(head);
        }

        private void PrintRow(string title, string value, bool error = false)
        {
            TableRow row = new TableRow();
            TableCell c0 = new TableCell { Text = title };
            TableCell c1 = new TableCell { Text = value };
            row.Cells.Add(c0);
            row.Cells.Add(c1);
            ContentTable.Rows.Add(row);
        }

        private void Separator()
        {
            TableRow separator = new TableRow();
            separator.Cells.Add(new TableCell { Text = "<br/>" });
            separator.Cells.Add(new TableCell { Text = "<br/>" });
            ContentTable.Rows.Add(separator);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //using (SPLongOperation longOperation = new SPLongOperation(this.Page))
            {
                SPSite site = SPContext.Current.Site;

                StringBuilder result = new StringBuilder();
                ConfigServiceClient configurations = null;
                RepoServiceClient scripts;
                SPList siteConfig = null;
                SPList siteScripts = null;

                IServiceLocator serviceLocator = SharePointServiceLocator.GetCurrent(site);
                IServiceLocatorConfig typeMappings = serviceLocator.GetInstance<IServiceLocatorConfig>();
                typeMappings.Site = site;

                #region Services

                PrintHeader("NVRServices");

                #region NVRConfigService

                string cfgSvcValue;

                try
                {
                    configurations = new ConfigServiceClient(site);
                    cfgSvcValue = configurations.ServiceApplicationIisGuid().ToString();
                }
                catch (Exception exc)
                {
                    cfgSvcValue = exc.Message;
                    return;
                }

                PrintRow("NVRConfigService", cfgSvcValue);

                #endregion

                #region NVRRepoService

                string scriptsSvcValue;

                try
                {
                    scripts = new RepoServiceClient(site);
                    scriptsSvcValue = scripts.ServiceApplicationIisGuid().ToString();
                }
                catch (Exception exc)
                {
                    scriptsSvcValue = exc.Message;
                }
                PrintRow("NVRRepoService", scriptsSvcValue);

                Separator();

                #endregion

                #endregion

                #region PageHeads

                PrintHeader("IPageHead");

                IEnumerable<IPageHead> pageHeadInstances = serviceLocator.GetAllInstances<IPageHead>();

                foreach (IPageHead pageInterface in pageHeadInstances)
                {
                    PrintRow(pageInterface.GetType().Name, "");
                }

                Separator();

                #endregion

                #region  ILOGGING

                PrintHeader("ILogging");

                IEnumerable<ILogging> logInstances = serviceLocator.GetAllInstances<ILogging>();

                if (logInstances.Count() == 0)
                {
                    PrintRow("NO LOGGING INTERFACE FOUND!!!", "");
                }

                foreach (ILogging logInterface in logInstances)
                {
                    PrintRow(logInterface.GetType().Name, "");
                }

                Separator();

                #endregion

                #region IExecute

                PrintHeader("IExecute");
                IEnumerable<IExecuteScript> executeInterfaces = serviceLocator.GetAllInstances<IExecuteScript>();
                if (executeInterfaces.Count() == 0)
                {
                    PrintRow("NO EXECUTE INTERFACE FOUND!!!", "");
                }

                foreach (IExecuteScript execInterface in executeInterfaces)
                {
                    PrintRow(execInterface.GetType().Name, "");
                }

                Separator();

                #endregion

                #region NVR LISTS

                PrintHeader("SiteConfig");

                #region SiteConfig

                try
                {
                    siteConfig = site.RootWeb.OpenList("SiteConfig", true);
                    PrintRow("SPList " + siteConfig.InternalName(), "OK");
                }
                catch (Exception exc)
                {
                    PrintRow(siteConfig.InternalName(), exc.Message);
                    return;
                }

                var siteConfigCt = siteConfig.ContentTypes.GetContentType("NVR_SiteConfigJSON");
                if (siteConfigCt == null)
                {
                    PrintRow("SPContentType NVR_SiteConfigJSON", "NOT FOUND");
                    return;
                }
                PrintRow("SPContentType NVR_SiteConfigJSON", "OK");

                try
                {
                    if (!siteConfig.ContainsFieldIntName(SiteStructuresDefinitions.SiteConfigFields)) throw new SPFieldNotFoundException(siteConfig, SiteStructuresDefinitions.SiteConfigFields);
                    PrintRow("Required Fields", "OK");
                }
                catch (Exception exc)
                {
                    PrintRow("Required Fields", exc.Message);
                }


                try
                {
                    SPListItem reloadItem = siteConfig.Items.Cast<SPListItem>().Where(i => i.ContentTypeId == siteConfigCt.Id).FirstOrDefault();
                    configurations.Reload(reloadItem);
                    PrintRow("SiteConfig Reload CFG", "OK");
                }
                catch (Exception exc)
                {
                    PrintRow("SiteConfig Reload CFG", exc.Message);
                }

                try
                {
                    var filters = new Dictionary<ConfigFilterType, string>
                    {
                        { ConfigFilterType.App, "JavaScripts" },
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

                    configurations.Filter(SPContext.Current.Web, filters);
                    PrintRow("SiteConfig Filter CFG", "OK");
                }
                catch (Exception exc)
                {
                    PrintRow("SiteConfig Filter CFG", exc.Message);
                }

                Separator();

                #endregion

                #region SiteScripts

                PrintHeader("SiteScripts");

                try
                {
                    siteScripts = site.RootWeb.OpenList("SiteScripts", true);
                    PrintRow("SPList " + siteScripts.InternalName(), "OK");
                }
                catch (Exception exc)
                {
                    PrintRow(siteScripts.InternalName(), exc.Message);
                    return;
                }

                var siteScriptsCt = siteScripts.ContentTypes.GetContentType("NVR_SiteScripts");
                if (siteScriptsCt == null)
                {
                    PrintRow("SPContentType NVR_SiteScripts", "NOT FOUND");
                }
                else
                {
                    PrintRow("SPContentType NVR_SiteScripts", "OK");
                }
               
                Separator();

                #endregion

                #endregion

                #region Plugins

                PrintHeader("Plugins");
                PluginHost.Reset();
                PluginHost.Init(site);
                List<string> plugins = PluginHost.List(site);

                foreach (var pluginName in plugins)
                {
                    IPlugin plugin = PluginHost.Get(site, pluginName);

                    PrintRow(plugin.Name(), plugin.Description());
                }

                #endregion

                /*
                longOperation.LeadingHTML = "Updating. . .";
                longOperation.TrailingHTML = "Totok se nezobrazilo";
                longOperation.Begin();
 * */


                //EndOperation(longOperation, System.Web.HttpUtility.JavaScriptStringEncode(result.ToString()));

                Content.Text = result.ToString();
            }
        }

        protected void EndOperation(SPLongOperation operation, string result)
        {
            HttpContext context = HttpContext.Current;
            if (context.Request.QueryString["IsDlg"] != null)
            {
                context.Response.Write("<script type='text/javascript'>alert('" + result + "'); window.frameElement.commitPopup();</script>");
                context.Response.Flush();
                context.Response.End();
            }
            else
            {
                string url = SPContext.Current.Web.Url;
                operation.End(url, SPRedirectFlags.CheckUrl, context, string.Empty);
            }
        }

    }
}