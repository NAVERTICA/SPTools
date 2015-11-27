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
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Navertica.SharePoint.Extensions;
using Newtonsoft.Json.Linq;

namespace Navertica.SharePoint
{
    public static class DeployTools
    {

        public static object[] FilesToList(FileInfo[] files, SPList list, Func<FileInfo, SPList, object> action)
        {
            List<object> results = new List<object>();

            foreach (FileInfo fi in files)
            {
                try
                {
                    results.Add(action(fi, list));
                }
                catch (Exception e)
                {
                    results.Add(e);
                }
            }

            return results.ToArray();
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
        /// <param name="overwrite"></param>
        public static string CopySiteConfigItems(SPSite site, string path, bool overwrite)
        {
            // mame adresar ze solution - v nem jsou soubory - jeden X.json a jeden X.metadata.json
            if (site == null) throw new ArgumentNullException("site");
            if (path == null) throw new ArgumentNullException("path");

            SPList siteConfigList = site.RootWeb.OpenList(Const.SITECONFIG_LIST_TITLE, true);

            List<string> log = new List<string>();

            DirectoryInfo di =
                new DirectoryInfo(string.Format(path, SPUtility.GetVersionedGenericSetupPath("Template", 15)));

            log.AddRange(FilesToList(di.GetFiles(), siteConfigList, delegate(FileInfo fi, SPList siteConfig)
            {
                if (fi.Name.ToLowerInvariant().EndsWith(".metadata.js") ||
                    fi.Name.ToLowerInvariant().EndsWith(".metadata.json") /*||
                    (!fi.Name.ToLowerInvariant().EndsWith(".js") ||
                     !fi.Name.ToLowerInvariant().EndsWith(".json"))*/
                    )
                {
                    return null;
                }
                if (fi.Name.ToLowerInvariant().EndsWith(".json") || fi.Name.ToLowerInvariant().EndsWith(".js"))
                {
                    Func<string, string> metadataContentFunc = delegate(string s)
                    {
                        FileInfo metadata = null;
                        List<string> splitName = new List<string>(s.Split('.'));
                        splitName.Insert(splitName.Count - 1, "metadata"); //orig -2
                        string metadataFileName = splitName.Join(".");

                        if (fi.Directory != null && fi.Directory.Exists)
                        {
                            metadata = fi.Directory.FindFileRecursive(metadataFileName);
                            if (metadata == null || !metadata.Exists) return null;
                        }
                        else return null;
                        return metadata.OpenText().ReadToEnd();
                    };

                    string metadataContent = metadataContentFunc(fi.Name);
                    string jsonContent = fi.OpenText().ReadToEnd();

                    //dynamic md = null;
                    JObject md;
                    try
                    {
                        md = JObject.Parse(metadataContent);
                    }
                    catch (Exception e)
                    {
                        return "failed parsing metadata file of " + fi.Name + "\n" + e.ToString();
                    }

                    try
                    {
                        md["SiteConfigJSON"] = JObject.Parse(jsonContent); //check-> ok
                    }
                    catch (Exception e)
                    {
                        return "failed parsing data of " + fi.Name + "\n" + e.ToString();
                    }

                    try
                    {
                        string title = md["Title"].Value<string>();
                        SPListItemCollection coll = siteConfig.GetItemsByTextField("Title", title, null); //prohozene parametry
                        SPListItem item = null;
                        if (coll.Count == 1)
                        {
                            item = coll[0];
                            if (!overwrite || ( (DateTime) item["Modified"] ) > fi.LastWriteTime)
                            {
                                log.Add("Skipping newer file " + fi.Name);
                                return log;
                            }
                        }
                        else if (coll.Count == 0)
                        {
                            item = siteConfig.Items.Add();
                            item["ContentTypeId"] = new SPContentTypeId("0x0100517300C02F16000000034700137001F7");
                            item["Title"] = title;
                        }
                        else
                        {
                            return "found more than one item with Title / filename " + fi.Name;
                        }

                        foreach (var name in Const.SiteConfigFields)
                        {
                            if (!item.Fields.ContainsFieldIntName(name)) continue;
                            try
                            {

                                //item[name] = ObjectExtensions.GetPropertyValue(md, name);
                                if (name == "SiteConfigJSON")
                                {
                                    var jt = md[name];
                                    item[name] = jt.Value<JObject>().ToString();
                                }
                                else
                                {
                                    item[name] = md[name].Value<string>(); //check -> ok
                                }

                                /***/
                                var ttt = item[name];
                                /***/
                            }
                            catch (Exception ee)
                            {
                                return "could not find property " + name + " in " + item.FormUrlDisplay() + "\n" +
                                       ee.ToString();
                            }
                        }
                        item.Web.AllowUnsafeUpdates = true; //not allowed for GET
                        item.Update();
                        item.Web.AllowUnsafeUpdates = false;
                    }
                    catch (Exception e)
                    {
                        return "exc\n" + e.ToString();
                    }

                    return fi.Name + " ok";
                } //endif
                else return fi.Name + " not ok"; //addition
            }).Select(o => ( o ?? string.Empty ).ToString()));

            return log.JoinStrings("\n");
        }
    }
}