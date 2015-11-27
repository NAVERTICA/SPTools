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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Newtonsoft.Json;

namespace Navertica.SharePoint.Extensions
{
    public static class VariousExtensions
    {
        /// <summary>
        /// Trims the beginning of the string as long as it matches the argument
        /// </summary>
        /// <param name="removeFrom"></param>
        /// <param name="toMatch"></param>
        /// <returns></returns>
        public static string RemoveMatchingStringFromStart(this string removeFrom, string toMatch)
        {
            if (string.IsNullOrWhiteSpace(removeFrom) || string.IsNullOrWhiteSpace(toMatch)) throw new ArgumentNullException();
            int position = 0;
            foreach (char ch in toMatch)
            {
                if (removeFrom[position] != ch) break;
                position++;
            }
            return removeFrom.Substring(position);
        }

        public static string GetTranslation(this string key, int lcid = -1, string resourceName = "Navertica")
        {
            if (key.Contains("$Resources"))
            {
                string[] val = key.Split(',');
                string[] res = val[0].Split(':');

                key = val[1];
                resourceName = res[1];
            }

            if (lcid < 0)
            {
                lcid = CultureInfo.CurrentUICulture.LCID;
            }

            return SPUtility.GetLocalizedString("$Resources:" + key, resourceName, (uint) lcid);
        }


        public static List<object> ImportFilesAsListItems(this SPList list, string path, string[] uniqueKeyFields, string keyForMainFileValue = null)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (list == null) throw new ArgumentNullException("list");

            List<object> results = new List<object>();

            DirectoryInfo di = new DirectoryInfo(string.Format(path, SPUtility.GetVersionedGenericSetupPath("Template", 15)));

            // walk all files, filter out non-json files, read the rest into a structure
            // TODO - not just a flat directory, but a folder structure
            Dictionary<string, string> filenamesAndData = new Dictionary<string, string>();
            foreach (FileInfo fi in di.GetFiles())
            {
                try
                {
                    string fname = fi.Name.ToLowerInvariant().Trim();

                    if (fname.EndsWith(".json"))
                    {
                        filenamesAndData.Add(fname,
                            Encoding.UTF8.GetString(fi.OpenRead().ReadFully())
                                .Replace( /* UTF8 BOM */Encoding.UTF8.GetString(new byte[] { 239, 187, 191 }), ""));
                    }
                }
                catch (Exception e)
                {
                    results.Add(e);
                }
            }

            foreach (KeyValuePair<string, string> kvp in filenamesAndData)
            {
                try
                {
                    if (kvp.Key.EndsWith(".metadata.json")) continue;
                    string jsonData = kvp.Value;

                    string noExt = kvp.Key.Substring(0, kvp.Key.LastIndexOf('.'));

                    Dictionary<string, object> metadata;
                    if (keyForMainFileValue != null)
                    {
                        if (!filenamesAndData.ContainsKey(noExt + ".metadata.json"))
                            throw new FileNotFoundException("Missing .metadata.json file for " + kvp.Key);

                        string metadataJsonString = filenamesAndData[noExt + ".metadata.json"];
                        metadata = JsonConvert.DeserializeObject<Dictionary<string, object>>(metadataJsonString);
                        metadata[keyForMainFileValue] = jsonData;
                    }
                    else
                    {
                        metadata = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
                    }

                    if (!metadata.ContainsKey("Title")) metadata["Title"] = noExt;

                    results.Add(list.AddOrUpdateItem(uniqueKeyFields, metadata));
                }
                catch (Exception e)
                {
                    results.Add(e);
                }

            }

            return results;
        }

        public static void UploadDirectoryRecursive(this SPFolder siteScripts, ref List<string> log, string hivePath, string filter = "*", string contentTypeNameOrId = "")
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(hivePath);
                log.Add("Adding dir " + di.FullName);

                foreach (FileInfo fi in di.GetFiles())
                {
                    try
                    {
                        if (( Path.GetExtension(fi.FullName).ToLowerInvariant() == filter || filter == "*" || fi.Name.Contains(filter.Substring(1)) ) && ( !siteScripts.Files.Web.GetFile(siteScripts.Url + '/' + fi.Name).Exists || fi.LastWriteTime.Ticks > siteScripts.Files[siteScripts.Url + '/' + fi.Name].TimeLastModified.Ticks ))
                        {
                            var item = siteScripts.UploadFile(log, hivePath, fi).Item;
                            if (!string.IsNullOrWhiteSpace(contentTypeNameOrId) && siteScripts.DocumentLibrary.ContentTypes.Contains(contentTypeNameOrId))
                            {
                                SPContentType ctp =
                                    siteScripts.DocumentLibrary.ContentTypes.GetContentType(contentTypeNameOrId);
                                if (ctp != null)
                                {
                                    item["ContentType"] = ctp;
                                }
                                // TODO nahravat i metadata
                                item.SystemUpdate(false, false);
                            }

                            log.Add("Uploaded file " + fi.Name + " as " + item.FormUrlDisplay());
                        }
                    }
                    catch (Exception e)
                    {
                        log.Add("Failed to upload file " + fi.Name + " \n" + e.ToString());
                    }
                }

                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    string d = dir.FullName.Substring(hivePath.Length).Replace("\\", "");
                    try
                    {
                        SPFolder serverSideFolder = siteScripts.GetOrCreateFolder(d);
                        serverSideFolder.UploadDirectoryRecursive(ref log, dir.FullName, filter);
                    }
                    catch (Exception ee)
                    {
                        log.Add("Failed to get or create folder" + dir.FullName + " \n" + ee.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                log.Add("Exception for " + hivePath + "\n" + e);
            }
        }

        public static SPFile UploadFile(this SPFolder folder, List<string> log, string hivePath, FileInfo fi)
        {
            folder.ParentWeb.AllowUnsafeUpdates = true;

            SPFile savedSPFile = folder.Files.Cast<SPFile>().Where(f => f.Name == fi.Name).SingleOrDefault();
            //log.Add("/" + fi.FullName);
            string saved = savedSPFile != null ? Encoding.UTF8.GetString(savedSPFile.OpenBinary()) : "";
            string copy = Encoding.UTF8.GetString(fi.OpenRead().ReadFully());

            if (saved != copy)
            {
                //try
                {
                    savedSPFile = folder.UploadFile(hivePath, fi.Name, true);
                    log.Add(fi.FullName);
                }
                //catch (Exception exc)
                {
                    //Tools.Log(exc);
                }
            }
            return savedSPFile;
        }

        public static byte[] ReadAllBytes(this Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}