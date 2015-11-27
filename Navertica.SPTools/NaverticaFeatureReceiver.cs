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
using Navertica.SharePoint.Extensions;
using Navertica.SharePoint.Interfaces;
using Navertica.SharePoint.Logging;
using Newtonsoft.Json;

namespace Navertica.SharePoint
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>
    public class NVRFeatureReceiver : SPFeatureReceiver
    {
        public ILogging log = NaverticaLog.GetInstance();

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            try
            {
                SPSite site = properties.Feature.Parent as SPSite;
                if (site == null) throw new NullReferenceException("Couldn't load parent site");
                // Scripts Service config properties
                SPFarm farm = properties.Definition.Farm;
                farm.Properties["NVR_ScriptsServiceBaseUrl"] = site.Url;
                farm.Update();

                // Copy files to SiteScripts and items to SiteConfig
                //site.RunElevated(delegate(SPSite spSite)
                {
                    try
                    {
                        CopySiteScriptsFilesToLibrary(site, //spSite,
                            properties.Definition.Properties["NVR_SiteScriptsFolder"].Value);
                    }
                    catch (Exception exc)
                    {
                        log.LogException(exc);
                        throw;
                    }

                    try
                    {
                        CopySiteConfigItems(site, //spSite,
                            properties.Definition.Properties["NVR_SiteConfigFolder"].Value,
                            "NVR_SiteConfigJSON");
                    }
                    catch (Exception exc)
                    {
                        log.LogException(exc);
                        throw;
                    }
                    //return null;
                }//);         
            }
            catch (Exception e)
            {
                log.LogException("exception in feature receiver of " + properties.Definition.DisplayName, e);
            }

        }

        /// <summary>
        /// Copies files from given directory to the SiteConfig list, overwriting existing items.
        /// 
        /// The filenames are split on __ (two underscores) and used to fill columns in the item.
        /// The format is "Location__App__.xml"
        /// 
        /// Contents of file go into the ConfigXML field.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="path">like @"{0}\LAYOUTS\SPTools\SiteConfigs\"</param>
        public void CopySiteConfigItems(SPSite site, string path, string keyForMainFileValue)
        {
            if (site == null) throw new ArgumentNullException("site");
            SPList siteConfig = site.RootWeb.OpenList(Const.SITECONFIG_LIST_TITLE, true);
            List<object> itemResults = siteConfig.ImportFilesAsListItems(path, new[] { "NVR_SiteConfigPackage", "Title" }, keyForMainFileValue);
            log.LogInfo("CopySiteConfigItems\n", itemResults.Select(i => i is SPListItem ? ((SPListItem)i).FormUrlDisplay() : i.ToString()).JoinStrings("\n"));
        }

        /// <summary>
        /// Copies files from given directory to the SiteScripts library, overwriting existing items.
        /// 
        /// There's a receiver on SiteScripts that syncs the content of file with a field in the item and vice versa.
        /// 
        /// The filenames are split on __ (two underscores) and used to fill columns in the item 
        /// using the script in Receiver__FileTextFieldSync__.py - the format is "App__Title__Variant.Language"
        /// </summary>
        /// <param name="site"></param>
        /// <param name="path">like @"{0}\LAYOUTS\SPTools\SiteScripts\"</param>
        /// <param name="filter">Use:"*" all files, ".js" only files with specified extension, "*Name" files containing specified string in name; default = "*"</param>
        public void CopySiteScriptsFilesToLibrary(SPSite site, string path, string filter = "*")
        {
            if (site == null) throw new ArgumentNullException("site");
            if (path == null) throw new ArgumentNullException("path");

            log.LogInfo("CopySiteScriptsFilesToLibrary starting");
            SPFolder siteScripts = site.RootWeb.Folders[Const.SITESCRIPTS_LIST_TITLE];

            List<string> treeLog = new List<string>();
            string hivePath = string.Format(path, SPUtility.GetVersionedGenericSetupPath("Template", 15)); //ve volani nahrazeno primo path

            siteScripts.UploadDirectoryRecursive(ref treeLog, hivePath, filter, "NVR_SiteScripts");
            log.LogInfo("CopySiteScriptsFilesToLibrary finished with results:\n", JsonConvert.SerializeObject(treeLog, Formatting.Indented));
        }
    }
}
