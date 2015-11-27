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
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Navertica.SharePoint.Extensions;
using Navertica.SharePoint.Logging;
using Navertica.SharePoint.RepoService;

namespace Navertica.SharePoint
{
    public class PluginHost
    {
        public static void Init(SPSite site)
        {
            List<string> allPluginNames = PluginHost.List(site);
            if (allPluginNames.Count == 0)
            {
                PluginHost.LoadPlugins(site);
            }
        }

        public static void Reset()
        {
            Collection = new Dictionary<string, List<IPlugin>>();
            ExecutePagePluginHost.Collection = new Dictionary<string, List<IPlugin>>();
            ItemReceiverPluginHost.Collection = new Dictionary<string, List<IPlugin>>();
            ListReceiverPluginHost.Collection = new Dictionary<string, List<IPlugin>>();
            TimerJobPluginHost.Collection = new Dictionary<string, List<IPlugin>>();
        }

        public static void LoadPlugins(SPSite site)
        {
            RepoServiceClient repo = new RepoServiceClient(site);

            var log = NaverticaLog.GetInstance();
            using (new SPMonitoredScope("LoadPlugins"))
            {
                foreach (string s in repo.FullPathsToAllFilesAndFolders)
                {
                    if (s.ToLowerInvariant().Contains("plugin"))
                    {
                        try
                        {
                            ScriptContext result = repo.Execute(s, new ScriptContext());
                            if (result != null)
                            {
                                foreach(string s1 in result.Keys)
                                {
                                    if (s1.StartsWith("_ERROR "))
                                    {
                                        log.LogError("Error loading plugin ", s, result[s1]);
                                    }                                    
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.LogError("Error loading plugin ", s, e);
                        }
                    }
                }
            }
        }

        //Kontrola jestli plugin obsahuje vsechny potrebne metody
        //TODO - nelze ale zjisit jestli metodu obsahuje jinak nez tim ze ji vola - nepouzivame tedy
        //
        public static bool ValidatePlugin(IPlugin plugin)
        {
            List<string> missingMethods = new List<string>();

            try
            {
                plugin.Name();
            }
            catch (MissingMemberException)
            {
                missingMethods.Add("Name");
            }

            try
            {
                plugin.Description();
            }
            catch (MissingMemberException)
            {
                missingMethods.Add("Description");
            }

            try
            {
                plugin.Version();
            }
            catch (MissingMemberException)
            {
                missingMethods.Add("Version");
            }

            try
            {
                //Tuto metodu pouzivame pouze v DLLExecute neni ji tedy nutne kontrolovatna DLR
                plugin.PluginScope();
            }
            catch (MissingMemberException)
            {
                missingMethods.Add("PluginScope");
            }

            try
            {
                plugin.Execute(null, null);
            }
            catch (MissingMemberException)
            {
                missingMethods.Add("Execute");
            }
            catch (Exception) {}


            try
            {
                plugin.Install(null, null);
            }
            catch (MissingMemberException)
            {
                missingMethods.Add("Install");
            }
            catch (Exception) { }

            try
            {
                plugin.Test(null, null);
            }
            catch (MissingMemberException)
            {
                missingMethods.Add("Test");
            }
            catch (Exception) {}

            try
            {
                plugin.GetInstance();
            }
            catch (Exception)
            {
                missingMethods.Add("GetInstance");
            }

            if (missingMethods.Count > 0)
            {
                var log = NaverticaLog.GetInstance();
                log.LogError(missingMethods.Contains("Name") ? "" : plugin.Name(), "Missing methods in plugin: " + missingMethods.JoinStrings(", "));
            }

            return missingMethods.Count == 0;
        }

        public static string Reload(string fileNamePath, SPSite site)
        {
            RepoServiceClient repo = new RepoServiceClient(site);
            fileNamePath = RepoServiceClient.NormalizedFilePath(fileNamePath);

            var log = NaverticaLog.GetInstance();
            using (new SPMonitoredScope("ReloadPlugin"))
            {
                if (repo.FullPathsToAllFilesAndFolders.Contains(fileNamePath) && fileNamePath.ToLowerInvariant().Contains("plugin"))
                {
                    try
                    {
                        var result = repo.Execute(fileNamePath, new ScriptContext());
                        if (result != null)
                        {
                            List<string> errors = new List<string>();
                            foreach(string s1 in result.Keys)
                                {
                                    if (s1.StartsWith("_ERROR "))
                                    {
                                        errors.Add(log.LogError("Error loading plugin ", s1, result[s1]));
                                    }                                    
                                }
                            
                            if (errors.Count > 0) return "Errors:\n" + string.Join("\n", errors);
                            return "Success " + fileNamePath;
                        }
                    }
                    catch (Exception ee)
                    {
                        return log.LogError("Exception loading plugin ", fileNamePath, ee);
                    }
                    return "Failed " + fileNamePath;
                }
                return "Not a file/plugin, skipped " + fileNamePath;
            }
        }

        #region overrides

        internal static Dictionary<string, List<IPlugin>> Collection = new Dictionary<string, List<IPlugin>>();

        public static void Add(SPSite site, IPlugin plugin, bool force = false)
        {
            if (site == null) throw new ArgumentNullException("site");
            if (plugin == null) throw new ArgumentNullException("plugin");
            //if (!ValidatePlugin(plugin)) return;

            if ((plugin.Name().Length > 0) && !(plugin.Name().Equals("All") || plugin.Name().Equals("None")))
            {
                if (List(site).Contains(plugin.Name()))
                {
                    if (force) Remove(site, plugin.Name());
                    else return;
                }

                if (!Collection.ContainsKey(site.Url))
                {
                    Collection.Add(site.Url, new List<IPlugin> {plugin});

                }
                else
                {
                    Collection[site.Url].Add(plugin);
                }
            }
        }

        public static List<string> List(SPSite site)
        {
            List<string> names = new List<string>();

            if (!Collection.ContainsKey(site.Url)) return names;

            foreach (IPlugin plugin in Collection[site.Url])
            {
                names.Add(plugin.Name());
            }
            return names;
        }

        public static IPlugin Get(SPSite site, string name)
        {
            foreach (IPlugin plugin in Collection[site.Url])
            {
                if (plugin.Name().Equals(name))
                {
                    IPlugin newInstance = plugin.GetInstance();
                    return newInstance;
                }
            }
            return null;
        }

        internal static void Remove(SPSite site, string name)
        {
            int pluginIndex = 0;
            for (int i = 0; i < Collection[site.Url].Count; i++)
            {
                if (Collection[site.Url][i].Name().Equals(name))
                {
                    pluginIndex = i;
                    break;
                }
            }
            Collection[site.Url].RemoveAt(pluginIndex);
        }

        #endregion

    }
}