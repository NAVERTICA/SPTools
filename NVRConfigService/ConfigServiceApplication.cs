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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text.RegularExpressions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using Navertica.SharePoint.Extensions;
using Navertica.SharePoint.Logging;
using Navertica.SharePoint.ConfigService.Administration;

namespace Navertica.SharePoint.ConfigService
{
    /// <summary>
    /// Just a list of config SPListItems' guids 
    /// </summary>
    public class ConfigGuidList : List<Guid>
    {
        public ConfigGuidList()
        {           
        }
        
        public ConfigGuidList(IEnumerable<Guid> lst)
        {
            this.AddRangeUnique(lst);
        }

        public ConfigGuidList(IEnumerable<ConfigGuidList> lstOfLists)
        {
            foreach(var lst in lstOfLists) this.AddRangeUnique(lst);
        }
    }

    /// <summary>
    /// Set of config SPListItems' guids for fast lookups where we don't need sorting
    /// </summary>
    public class ConfigGuidSet : HashSet<Guid>
    {
        public ConfigGuidSet()
        {
        }

        public ConfigGuidSet(IEnumerable<Guid> lst)
        {
            this.AddRangeUnique(lst);
        }

        public ConfigGuidSet(IEnumerable<ConfigGuidSet> lstOfLists)
        {
            foreach (var lst in lstOfLists) this.AddRangeUnique(lst);
        }
    }
}

namespace Navertica.SharePoint.ConfigService.Service
{
    /// <summary>
    /// The Service Application that really does the job of loading, caching and querying
    /// all the configurations that are available in SiteConfig. There must be only one instance 
    /// of this service running in the farm. It is accessed using <see cref="Navertica.SharePoint.ConfigService.ConfigServiceClient"/> 
    /// or (TODO) with REST services
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = true)]
    [IisWebServiceApplicationBackupBehavior]
    [System.Runtime.InteropServices.Guid("aeed812c-7b48-47da-b5f3-143d42dcda4a")]
    internal sealed class ConfigServiceApplication : SPIisWebServiceApplication
    {
        #region Data and indexes
        /// <summary>
        /// Main data structure, <code>keys</code> are SPListItem Guids of all listitems in farm with NVR_SiteConfigJSON content typ,
        /// <code>values</code> are <see cref="ConfigEntry"/> types, which hold all the necessary information from the item.
        /// </summary>
        private Dictionary<Guid, ConfigEntry> _configs = new Dictionary<Guid, ConfigEntry>();
        
        /// <summary>
        /// <para>This structure deals with holding urls of webs as keys, and as value there's another dictionary 
        /// with (again) the urls of webs as keys and lists of NVR_SiteConfigJSON Guids as values.</para>
        /// <para>In simple configurations with a single SiteConfig list per site there will be just one instance of the
        /// inner dictionary with one key (url of root web with the only SiteConfig) and a single list of guids,
        /// while the outer dictionary will hold many keys-web urls, but all of them will reference the single inner instance. </para>
        /// <para>In sites with SiteConfig lists on different webs and subwebs, this enables to always include
        /// all the entries from the subweb's SiteConfig and from all its parent web's SiteConfig's up to the root one.</para>
        /// </summary>
        private Dictionary<string, Dictionary<string, ConfigGuidSet>> _websAndItems = new Dictionary<string, Dictionary<string, ConfigGuidSet>>();


        // Primitive indexes linking Guids of SPListItems to strings from NVR_SiteConfigJSON fields 
        private Dictionary<Guid, List<string>> _indexUrl = new Dictionary<Guid, List<string>>();
        private Dictionary<Guid, List<string>> _indexListType = new Dictionary<Guid, List<string>>();
        private Dictionary<Guid, List<string>> _indexContentType = new Dictionary<Guid, List<string>>();
        private Dictionary<Guid, List<string>> _indexApp = new Dictionary<Guid, List<string>>();

        // GUID of NVR_SiteConfigJSON SPListItem and the value of its SiteConfigOrder field
        private Dictionary<Guid, int> _indexOrder = new Dictionary<Guid, int>();
        const string DefaultOrder = "1000";

        // Cache for the RegExps we get from processed wildcard strings
        private Dictionary<string, Regex> _wildcardStringsToRegexRepo = new Dictionary<string, Regex>();

        /// <summary>
        /// Given a SharePoint web URL, it prepares a list of SPListItem Guids valid for that URL 
        /// - walking through parent webs up to the root, it includes items from every encountered SiteConfig.
        /// </summary>
        /// <param name="siteWebUrl">SharePoint web/subweb URL</param>
        /// <returns>List of SPListItem Guids to be filtered</returns>
        private Dictionary<string, ConfigGuidSet> GetItemsForWebAndParents(string siteWebUrl)
        {
            var validIDsForUrl = new Dictionary<string, ConfigGuidSet>();

            using (var sitetemp = new SPSite(siteWebUrl))
            {
                sitetemp.RunElevated(delegate(SPSite site)
                {
                    using (var web = site.OpenW(siteWebUrl))
                    {
                        WebListId siteConfigListIdent = web.FindList("SiteConfig");

                        if (!siteConfigListIdent.IsValid) throw new SPListNotFoundException("SiteConfig", web);

                        using (var w = siteConfigListIdent.OpenWeb(site))
                        {
                            // in case we have all the data already
                            if (_websAndItems.ContainsKey(w.Url)) return _websAndItems[w.Url];

                            validIDsForUrl[w.Url] = new ConfigGuidSet();

                            // otherwise we gather all the data from  NVR_SiteConfigJSON items 
                            siteConfigListIdent.OpenList(w)
                                .ProcessItems(
                                    i => i.ContentType.Name == "NVR_SiteConfigJSON" ? i.GetItemDictionary() : null)
                                .Cast<DictionaryNVR>()
                                .ForEach(delegate(DictionaryNVR i)
                                {
                                    if (i == null || w == null) return;

                                    Guid id = UpdateItem(i, w);

                                    validIDsForUrl[w.Url].AddUnique(id);
                                });

                            // recursion until we reach root web
                            if (w.ParentWeb != null)
                            {
                                GetItemsForWebAndParents(w.ParentWeb.Url)
                                    .ForEach(r =>
                                        validIDsForUrl[r.Key] = r.Value);
                            }
                        }
                    }
                    return null;
                });
            }

            return validIDsForUrl;
        }

        /// <summary>
        /// Given a dictionary with all necessary data from the NVR_SiteConfigJSON item
        /// </summary>
        /// <param name="itemData">dictionary with data from the NVR_SiteConfigJSON item</param>
        /// <param name="web">SharePoint parent web of the item</param>
        /// <returns>Guid of the updated item</returns>
        public Guid UpdateItem(DictionaryNVR itemData, SPWeb web)
        {
            Guid id = (Guid) itemData["_ItemUniqueId"];

            // store data
            _configs[id] = new ConfigEntry(itemData, web);

            // generate indexes
            GenerateIndex(_indexUrl, id, (itemData["NVR_SiteConfigUrl"] ?? "" ).ToString().SplitByChars("|\n").ToList());
            GenerateIndex(_indexListType, id, (itemData["NVR_SiteConfigContentType"] ?? "" ).ToString().SplitByChars("\n").ToList());
            GenerateIndex(_indexContentType, id, (itemData["NVR_SiteConfigListType"] ?? "" ).ToString().SplitByChars("\n").ToList());
            GenerateIndex(_indexApp, id, (itemData["NVR_SiteConfigApp"] ?? "" ).ToString().SplitByChars(",;\n").ToList());

            _indexOrder[id] = int.Parse((itemData["NVR_SiteConfigOrder"] ?? DefaultOrder ).ToString());

            return id;
        }

        /// <summary>
        /// Populates internal indexes
        /// </summary>
        private void GenerateIndex(Dictionary<Guid, List<string>> itemIndex, Guid id, IEnumerable<string> strArr)
        {
            if (!itemIndex.ContainsKey(id)) itemIndex[id] = new List<string>();
            itemIndex[id].AddRangeUnique(strArr);
        }

        /// <summary>
        /// After the SPListItem has been modified, this ensures all the data and indexes stay updated 
        /// </summary>
        /// <param name="siteWebUrl"></param>
        /// <param name="relativeItemUrl"></param>
        public void ReloadItem(string siteWebUrl, string relativeItemUrl)
        {
            using (SPSite tempsite = new SPSite(siteWebUrl))
            {
                tempsite.RunElevated(delegate(SPSite site)
                {
                    using (SPWeb web = site.OpenW(siteWebUrl))
                    {
                        if (!_websAndItems.ContainsKey(web.Url))
                            _websAndItems[web.Url] = GetItemsForWebAndParents(web.Url);

                        SPListItem i = web.GetListItem(relativeItemUrl);

                        var fieldNames = new[]
                        {
                            "_ItemUniqueId",
                            "NVR_SiteConfigApp", "NVR_SiteConfigUrl", "NVR_SiteConfigContentType",
                            "NVR_SiteConfigListType",
                            "NVR_SiteConfigOrder", "NVR_SiteConfigJSON", "NVR_SiteConfigActive",
                            "NVR_SiteConfigApproved",
                            "NVR_SiteConfigActiveFor"
                        };

                        var newItemData = i.GetItemDictionary(fieldNames);

                        Guid id = UpdateItem(newItemData, web);

                        // do zaznamu pro podweby 
                        if (!_websAndItems[web.Url][web.Url].Contains(id)) _websAndItems[web.Url][web.Url].Add(id);
                    }
                    return null;
                });
            }

        }

        /// <summary>
        /// Sort items in list based on the value of NVR_SiteConfigOrder field value
        /// </summary>
        /// <param name="items"></param>
        /// <returns>Sorted ConfigGuidList</returns>
        public ConfigGuidList Sort(ConfigGuidList items)
        {
            Dictionary<Guid, int> results = new Dictionary<Guid, int>();
            foreach (Guid g in items)
            {
                int sortVal;
                _indexOrder.TryGetValue(g, out sortVal);
                results[g] = sortVal;
            }
            List<KeyValuePair<Guid, int>> sortlist = results.ToList();

            sortlist.Sort((firstpair, nextpair) => firstpair.Value.CompareTo(nextpair.Value));

            return new ConfigGuidList(sortlist.Select(i => i.Key).ToList());
        }

        /// <summary>
        /// Sort items in list based on the value of NVR_SiteConfigOrder field value
        /// </summary>
        /// <param name="items"></param>
        /// <returns>Sorted ConfigGuidList</returns>
        public ConfigGuidList Sort(ConfigGuidSet items)
        {
            Dictionary<Guid, int> results = new Dictionary<Guid, int>();
            foreach (Guid g in items)
            {
                int sortVal;
                _indexOrder.TryGetValue(g, out sortVal);
                results[g] = sortVal;
            }
            List<KeyValuePair<Guid, int>> sortlist = results.ToList();

            sortlist.Sort((firstpair, nextpair) => firstpair.Value.CompareTo(nextpair.Value));

            return new ConfigGuidList(sortlist.Select(i => i.Key).ToList());
        }

        /// <summary>
        /// <para>Given a list or set of NVR_SiteConfigJSON item's Guids, and wildcard values to filter by,
        /// checks which items pass the filter and returns them.</para>
        /// <para>Before calling Filter for the first time, AllConfigsForWeb must've been called.</para>
        /// </summary>
        /// <param name="items">list of NVR_SiteConfigJSON item Guids</param>
        /// <param name="filterByField">what to filter by</param>
        /// <param name="value">filter value (usually with ? and * wildcards)</param>
        /// <param name="forUserLogin">include those active only for given user</param>
        /// <returns></returns>
        public ConfigGuidSet Filter(ConfigGuidSet items, ConfigFilterType filterByField, string value, string forUserLogin)
        {
            var filteredItemSet = new ConfigGuidSet();
            string processedFilterValue = value;

            foreach (Guid g in ( filteredItemSet.Count == 0 ? items : filteredItemSet ))
            {
                List<string> allValuesInCurrentField;
                switch (filterByField)
                {
                    case ConfigFilterType.App:
                        _indexApp.TryGetValue(g, out allValuesInCurrentField);
                        break;
                    case ConfigFilterType.Url:
                        _indexUrl.TryGetValue(g, out allValuesInCurrentField);
                        if (processedFilterValue.StartsWith("http"))
                            processedFilterValue = new Uri(processedFilterValue).PathAndQuery;
                        break;
                    case ConfigFilterType.ContentType:
                        _indexContentType.TryGetValue(g, out allValuesInCurrentField);
                        break;
                    case ConfigFilterType.ListType:
                        _indexListType.TryGetValue(g, out allValuesInCurrentField);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("filterByField");
                }

                if (allValuesInCurrentField != null && allValuesInCurrentField.Count != 0 &&
                    !( allValuesInCurrentField.Count == 1 && string.IsNullOrEmpty(allValuesInCurrentField[0]) ))
                {
                    bool negativeMatch = false;
                    bool excluded = false;
                    bool matched = false;

                    foreach (string possibility in allValuesInCurrentField)
                    {
                        string fieldValueToFilter = possibility;

                        // if there's a minus (-) at the start of this value, 
                        // this item will NOT be included if it passes the filter
                        // even if it passes other filters
                        if (fieldValueToFilter.StartsWith("-"))
                        {
                            negativeMatch = true;
                            fieldValueToFilter = fieldValueToFilter.Remove(0, 1);
                        }

                        if (GetRegex
                            (fieldValueToFilter).IsMatch(processedFilterValue))
                        {
                            if (negativeMatch)
                            {
                                excluded = true;
                                break;
                            }
                            matched = true;
                        }
                    }
                    if (matched && !excluded)
                    {
                        if (_configs[g].Active && (_configs[g].ActiveForUsers == null || _configs[g].ActiveForUsers.Contains(forUserLogin)))
                            filteredItemSet.Add(g);
                    }
                }
                else // there's nothing in the field to pass to the filter - item is included                     
                    if (_configs[g].Active && (_configs[g].ActiveForUsers == null || _configs[g].ActiveForUsers.Contains(forUserLogin)))
                        filteredItemSet.Add(g);
            }

            return filteredItemSet;
        }

        /// <summary>
        /// <para>Given a list of NVR_SiteConfigJSON item's Guids, and wildcard values to filter by,
        /// checks which items pass the filter and returns them.</para>
        /// </summary>
        /// <param name="items">list of NVR_SiteConfigJSON item Guids</param>
        /// <param name="filters">what to filter by</param>
        /// <param name="forUserLogin">include those active only for given user</param>
        /// <returns></returns>
        public ConfigGuidSet Filter(ConfigGuidSet items, Dictionary<ConfigFilterType, string> filters, string forUserLogin)
        {
            ConfigGuidSet results = new ConfigGuidSet(items);

            foreach (KeyValuePair<ConfigFilterType, string> kvp in filters)
            {
                results = Filter(results, kvp.Key, kvp.Value, forUserLogin);
            }

            return results;
        }

        /// <summary>
        /// Load's all NVR_SiteConfigJSON config items of all SiteConfig lists in given web and its ParentWebs
        /// </summary>
        /// <param name="siteWebUrl"></param>
        /// <returns></returns>
        public ConfigGuidSet AllConfigsForWeb(string siteWebUrl)
        {
            if (!_websAndItems.ContainsKey(siteWebUrl))
                _websAndItems[siteWebUrl] = GetItemsForWebAndParents(siteWebUrl);
            return new ConfigGuidSet(_websAndItems[siteWebUrl].Select(i => i.Value));
        }

        /// <summary>
        /// Calculates a regex from a wildcard string
        /// </summary>
        /// <param name="possibleMatch"></param>
        /// <returns></returns>
        private Regex GetRegex(string possibleMatch)
        {
            if (!_wildcardStringsToRegexRepo.ContainsKey(possibleMatch))
            {
                string pattern =
                    '^' +
                    Regex.Escape(possibleMatch
                        .Replace("*", "__STAR__")
                        .Replace("?", "__QM__"))
                        .Replace("__STAR__", ".*")
                        .Replace("__QM__", ".")
                    + '$';

                _wildcardStringsToRegexRepo[possibleMatch] = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            return _wildcardStringsToRegexRepo[possibleMatch];
        }

        /// <summary>
        /// For config item with given Guid ID, return the JSON string for branch name
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetBranchJson(Guid itemId, string name)
        {
            try
            {
                ConfigEntry entry = _configs[itemId];
                if (entry != null) return entry.GetBranchJson(name);
            }
            catch (Exception e)
            {
                try
                {
                    var log = NaverticaLog.GetInstance();
                    log.LogException(e);
                }
                catch
                {
                    throw e;
                }
            }
            return "";
        }

        /// <summary>
        /// Reset all data
        /// </summary>
        /// <param name="siteWebUrl"></param>
        public void Reset(string siteWebUrl)
        {
            _configs = new Dictionary<Guid, ConfigEntry>();
            _websAndItems = new Dictionary<string, Dictionary<string, ConfigGuidSet>>();
            _indexUrl = new Dictionary<Guid, List<string>>();
            _indexListType = new Dictionary<Guid, List<string>>();
            _indexContentType = new Dictionary<Guid, List<string>>();
            _indexApp = new Dictionary<Guid, List<string>>();
            _indexOrder = new Dictionary<Guid, int>();
            _wildcardStringsToRegexRepo = new Dictionary<string, Regex>();
        }

        public Guid ServiceApplicationIisGuid()
        {
            return this.ApplicationPool.Id;
        }

        #endregion

        #region Plumbing

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigServiceApplication"/> class. Default constructor (required for SPPersistedObject serialization). Never call this directly.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ConfigServiceApplication() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigServiceApplication"/> class. Use this constructor when creating a new Service Application (e.g. from code in your Create page)
        /// </summary>
        /// <param name="name">The name of the service application.</param>
        /// <param name="service">The <see cref="NVRConfigService" />.</param>
        /// <param name="applicationPool">The application pool.</param>
        internal ConfigServiceApplication(string name, NVRConfigService service, SPIisWebServiceApplicationPool applicationPool)
            : base(name, service, applicationPool) {}

        #endregion

        #region Properties

        /// <summary>
        /// Gets the TypeName. This string will display in the Type column on the Manage Service Applications screen. You can localize this value. If you don't override this, 
        /// the default string in the Type column will be the name of this type from GetType().
        /// </summary>
        public override string TypeName
        {
            get { return SPUtility.GetLocalizedString("$Resources:ServiceApplicationName", "NVRConfigService.ServiceResources", (uint) System.Threading.Thread.CurrentThread.CurrentCulture.LCID); }
        }

        /// <summary>
        /// Gets the link to the Management page for this service application. Use this page to provide a UI for changing and configuring your application-specific settings.
        /// </summary>
        public override SPAdministrationLink ManageLink
        {
            get { return new SPAdministrationLink(string.Format(CultureInfo.InvariantCulture, "/_admin/NVRConfigService/ManageApplication.aspx?{0}", SPHttpUtility.UrlKeyValueEncode("id", this.Id.ToString()))); }
        }

        /// <summary>
        /// Gets the link to the Properties page for this service application. Use this page to enable the user to change basic settings such as the Application Pool.
        /// </summary>
        public override SPAdministrationLink PropertiesLink
        {
            get { return new SPAdministrationLink(string.Format(CultureInfo.InvariantCulture, "/_admin/NVRConfigService/Properties.aspx?{0}", SPHttpUtility.UrlKeyValueEncode("id", this.Id.ToString()))); }
        }

        /// <summary>
        /// Gets the current version number of this service application. This number should match the number in the ServiceProxy's SupportedServiceApplication attribute.
        /// </summary>
        public override Version ApplicationVersion
        {
            get { return new Version("1.0.0.0"); }
        }

        /// <summary>
        /// Gets the Class Id. This is used in the SupportedServiceApplication attribute.
        /// </summary>
        public override Guid ApplicationClassId
        {
            get { return new Guid("c0341fd5-43b5-4f2c-be52-4e2912a20a77"); }
        }

        /// <summary>
        /// Gets the Virtual Path of the service. The path to your services SVC file. 
        /// </summary>
        /// <remarks>
        /// Service applications only support one SVC file. To support multiple svc files in a single service application, 
        /// use a placeholder string here, and replace it in your Service Application Client at runtime after the load-balanced url has been acquired.
        /// </remarks>
        protected override string VirtualPath
        {
            get { return "NVRConfigService.svc"; }
        }

        /// <summary>
        /// Gets the install path to the subfolder of the WebServices folder.
        /// </summary>
        protected override string InstallPath
        {
            get { return SPUtility.GetVersionedGenericSetupPath(@"WebServices\NVRConfigService", 15); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// This is responsible for provisioning the Service Application. This override is where you can install any timer jobs or other application components.
        /// </summary>
        public override void Provision()
        {
            // First change the status of the object, this is not done in the base class implementations of Provision.
            if (SPObjectStatus.Provisioning != this.Status)
            {
                this.Status = SPObjectStatus.Provisioning;
                this.Update();
            }

            // Call the base implementation. The base class will update the object and set its status to Online.
            base.Provision();
        }

        /// <summary>
        /// Removes the Service Application. If using a custom persisted database, include logic to remove the DB as well here. Don't forget to call Delete afterwards.
        /// </summary>
        /// <param name="deleteData">Whether to delete data associated with this service application.</param>
        public override void Unprovision(bool deleteData)
        {
            // First mark the status, this is not done in the base class implementations of Unprovision
            if (SPObjectStatus.Unprovisioning != this.Status)
            {
                this.Status = SPObjectStatus.Unprovisioning;
                this.Update();
            }

            // Unprovision this parent object first. The base class will Update the object and set its status to Disabled.
            base.Unprovision(deleteData);
        }

        /// <summary>
        /// Deletes the Service Application from the Persisted Object Store. Use this to delete child objects such as custom 
        /// databases or Timer Jobs.
        /// </summary>
        public override void Delete()
        {
            // Delete this parent object first, otherwise we can't delete dependent objects like a Database later.
            base.Delete();
        }

        /// <summary>
        /// This will be called during initial provisioning, and if the Application Pool is ever changed.
        /// </summary>
        /// <param name="processSecurityIdentifier">A security identifier.</param>
        protected override void OnProcessIdentityChanged(SecurityIdentifier processSecurityIdentifier)
        {
            base.OnProcessIdentityChanged(processSecurityIdentifier);
        }

        #endregion

        #endregion
    }
}