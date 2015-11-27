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
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.ConfigService.Service;
using Navertica.SharePoint.Extensions;

namespace Navertica.SharePoint.ConfigService
{
    /// <summary>
    /// This is the class that is accessible to the Client callers 
    /// (web parts, user controls, timer jobs, etc.). It just directs
    /// the calls through the <see cref="ConfigurationsWCFService"/> to the <see cref="ConfigServiceApplication"/>
    /// </summary>
    public class ConfigServiceClient : BaseServiceClient
    {
        #region Methods

        /// <summary>
        /// Tells the service an item in SiteConfig has been updated and should be reloaded.
        /// </summary>
        /// <param name="item"></param>
        public void Reload(SPListItem item)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (item.ContentType.Name != "NVR_SiteConfigJSON")
                throw new ArgumentException("item doesn't seem to be a config item");

            this.ExecuteOnChannel(delegate(IConfigurationsWCFService channel)
            {
                channel.Reload(item.Web.Url, item.Url);
            }, false);
        }

        /// <summary>
        /// Returns the list of config item Guids for a given appName, item's content type, 
        /// parent list base template, default view url and current user login
        /// </summary>
        /// <param name="item"></param>
        /// <param name="appName"></param>
        /// <returns></returns>
        public ConfigGuidList Filter(SPListItem item, string appName)
        {
            if (item == null) throw new ArgumentNullException("item");

            ConfigGuidList response = null;
            #if DEBUG
            using (new SPMonitoredScope("Config filter"))
            #endif
            {
                var filterSettings = new Dictionary<ConfigFilterType, string>
                {
                    { ConfigFilterType.App, appName },
                    { ConfigFilterType.ContentType, item.ContentTypeId.ToString() },
                    { ConfigFilterType.ListType, item.ParentList.BaseTemplate.ToString() },
                    { ConfigFilterType.Url, item.ParentList.DefaultViewUrl }
                };

                this.ExecuteOnChannel<IConfigurationsWCFService>(
                    delegate(IConfigurationsWCFService channel)
                    {
                        response = channel.Filter(item.Web.Url, filterSettings, item.Web.CurrentUser.LoginNameNormalized());
                    },
                    false);
            }
            return response;
        }

        /// <summary>
        /// Queries the configurations in the service for a list of items that pass the given filters.
        /// </summary>
        /// <param name="web"></param>
        /// <param name="filterSettings">URLs to filter by must be RELATIVE, with leading slash</param>
        /// <returns></returns>
        public ConfigGuidList Filter(SPWeb web, Dictionary<ConfigFilterType, string> filterSettings)
        {
            if (web == null) throw new ArgumentNullException();
            ConfigGuidList response = null;
            using (new SPMonitoredScope("Config filter"))
            {
                this.ExecuteOnChannel<IConfigurationsWCFService>(
                    delegate(IConfigurationsWCFService channel)
                    {
                        response = channel.Filter(web.Url, filterSettings, web.CurrentUser.LoginNameNormalized());
                    },
                    false);
            }
            return response;
        }

        /// <summary>
        /// Use this to get an initialized instance of your own <see cref="ConfigBranch"/>-inheriting class
        /// </summary>
        /// <typeparam name="T">must inherit <see cref="ConfigBranch"/></typeparam>
        /// <param name="itemId"></param>
        /// <param name="branchName"></param>
        /// <returns>your initialized config class</returns>
        public ConfigBranch GetBranch<T>(Guid itemId, string branchName) where T : ConfigBranch, new()
        {
            #if DEBUG
            using (new SPMonitoredScope("GetBranch " + branchName))
            #endif
            {
                string json = GetBranchJson(itemId, branchName);
                if (string.IsNullOrEmpty(json)) return null;

                ConfigBranch result = new T();
                result.Json = json;

                return (T) result.Initialize();
            }
        }

        /// <summary>
        /// Get a branch of config item as JSON string
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="branchName"></param>
        /// <returns></returns>
        public string GetBranchJson(Guid itemId, string branchName)
        {
            string response = null;

            try
            {
                this.ExecuteOnChannel<IConfigurationsWCFService>(
                    delegate(IConfigurationsWCFService channel)
                    {
                        response = channel.GetBranchJson(itemId, branchName);
                    },
                    false);
            }
            catch (Exception e)
            {
                Logging.NaverticaLog.GetInstance().LogWarning(string.Format("item id {0} doesn't contain json branch named {1} or other problem\n{2}", itemId, branchName, e.ToString()));
            }

            return response;
        }

        /// <summary>
        /// Drop and reload all config data
        /// </summary>
        public void Reset(string siteWebUrl)
        {
            this.ExecuteOnChannel<IConfigurationsWCFService>(
                delegate(IConfigurationsWCFService channel)
                {
                    channel.Reset(siteWebUrl);
                },
                false);
        }

        public Guid ServiceApplicationIisGuid()
        {
            Guid r = Guid.Empty;
            this.ExecuteOnChannel<IConfigurationsWCFService>(
                delegate(IConfigurationsWCFService channel)
                {
                   r = channel.ServiceApplicationIisGuid();
                },
                false);
            return r;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigServiceClient"/> class.
        /// </summary>
        [Obsolete]
        public ConfigServiceClient() : base() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigServiceClient"/> class.
        /// </summary>
        /// <param name="site"></param>
        public ConfigServiceClient(SPSite site) : base(SPServiceContext.GetContext(site)) {}

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the actual (.svc file) for this service.
        /// </summary>
        /// <remarks>
        /// Service applications are designed to support a single endpoint .svc file. For more complicated 
        /// service applications with many different types of services, it makes sense to create several .svc files 
        /// and classes. To support multiple end points, use a recognizable string here, and swap it out dynamically 
        /// in the BaseServiceClient's GetEndPoint method after the load balancer has provided the full path to this 
        /// original end point.
        /// </remarks>
        protected override string EndPoint
        {
            get { return "NVRConfigService.svc"; }
        }

        #endregion

    }
}