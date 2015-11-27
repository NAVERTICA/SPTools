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
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.Extensions;
using Navertica.SharePoint.Interfaces;
using Navertica.SharePoint.Logging;
using Newtonsoft.Json.Linq;

namespace Navertica.SharePoint
{
    public class TimerJob : SPJobDefinition
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the DeadlineTimerJob class.
        /// </summary>
        public TimerJob() {}

        /// <summary>
        /// Initializes a new instance of the DeadlineTimerJob class.
        /// </summary>
        /// <param name="jobName">Name of the job.</param>
        /// <param name="service">The service.</param>
        /// <param name="server">The server.</param>
        /// <param name="targetType">Type of the target.</param>
        public TimerJob(string jobName, SPService service, SPServer server, SPJobLockType targetType) : base(jobName, service, server, targetType) {}

        /// <summary>
        /// Initializes a new instance of the DeadlineTimerJob class.
        /// </summary>
        /// <param name="jobName">Name of the job.</param>
        /// <param name="webApplication">The web application.</param>
        /// <param name="server"></param>
        /// <param name="targetType"></param>
        public TimerJob(string jobName, SPWebApplication webApplication, SPServer server, SPJobLockType targetType) : base(jobName, webApplication, server, targetType)
        {
            //Title = this.jobName;
        }

        /// <summary>
        /// Initializes a new instance of the DeadlineTimerJob class.
        /// </summary>
        /// <param name="jobName">Name of the job.</param>
        /// <param name="webApplication">The web application.</param>
        public TimerJob(string jobName, SPWebApplication webApplication) : base(jobName, webApplication, null, SPJobLockType.ContentDatabase)
        {
            //Title = this.JobName;
        }

        #endregion

        /// <summary> 
        /// Executes the specified content db id.
        /// </summary>
        /// <param name="contentDbId">The content db id.</param>
        public override void Execute(Guid contentDbId)
        {
            ILogging log = NaverticaLog.GetInstance();
            List<KeyValuePair<string, object>> executionResults = new List<KeyValuePair<string, object>>();

            SPWebApplication webApplication = this.Parent as SPWebApplication;
            if (webApplication == null)
            {
                log.LogError(this.Name + ": cannot get SPWebApplication!!!");
                return;
            }

            //Vetsinou je jen jedna ale muze jich byt vice
            //SPContentDatabase contentDb = webApplication.ContentDatabases[0];
            foreach (SPContentDatabase contentDb in webApplication.ContentDatabases)
            {
                for (int i = 0; i < contentDb.Sites.Count; i++)
                {
                    using (SPSite site = contentDb.Sites[i])
                    {
                        try
                        {
                            ConfigServiceClient configurations = new ConfigServiceClient(site);

                            log.LogInfo("JOB " + this.Name + " RAN at site: " + site.Url);

                            ConfigGuidList cfgs = configurations.Filter(site.RootWeb, new Dictionary<ConfigFilterType, string> { { ConfigFilterType.App, this.Name } });

                            if (cfgs.Count == 0) continue;

                            PluginHost.Init(site);

                            foreach (var cfgguid in cfgs)
                            {
                                PluginConfig cfg = (PluginConfig) configurations.GetBranch<PluginConfig>(cfgguid, "TimerJob");

                                if (cfg == null) continue;

                                foreach (KeyValuePair<string, KeyValuePair<string, JObject>> kvp in cfg.PluginConfigs)
                                {
                                    string pluginName = kvp.Key;

                                    IPlugin plugin = TimerJobPluginHost.Get(site, pluginName);
                                    if (plugin == null)
                                    {
                                        log.LogError(string.Format(
                                            "TimerJobPlugin named {0} was not found in loaded plugins, skipping execution",
                                            pluginName));
                                        continue;
                                    }

                                    TimerJobPluginConfig timerPluginConfig = ConfigBranch.GetInstance<TimerJobPluginConfig>(kvp.Value.Value.ToString());

                                    if (!timerPluginConfig.EnabledToRun) continue;

                                    try
                                    {
                                        using (new SPMonitoredScope("Executing TimerJobPlugin " + pluginName))
                                        {
                                            object result = plugin.Execute(timerPluginConfig, site);
                                            executionResults.Add(new KeyValuePair<string, object>(pluginName + "|" + kvp.Value.Key,
                                                result));
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        log.LogError(string.Format("TimerJobPlugin named {0} has thrown {1}", pluginName, e, e.InnerException, e.StackTrace, e.Source));
                                    }
                                }
                            }

                            string logstr =
                                log.LogInfo("TimerJobPlugins ran:\n" + executionResults.Select(
                                    kvp =>
                                        string.Format("Ran TimerJobPlugin|config name {0} with result:{1}\n\n", kvp.Key,
                                            ( kvp.Value ?? "null" ).ToString())).JoinStrings("\n"));
                        }
                        catch (Exception e)
                        {
                            log.LogException(e);
                        }
                    }
                }
            }
        }
    }
}