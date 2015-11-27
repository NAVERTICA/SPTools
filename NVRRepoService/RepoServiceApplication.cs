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
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using Navertica.SharePoint.Extensions;

namespace Navertica.SharePoint.RepoService.Service
{
    /// <summary>
    /// The Service Application that really does the job of loading and caching
    /// all the scripts that are available in SiteScripts library of a site. There must be only one instance 
    /// of this service running in the farm. It is accessed using <see cref="Navertica.SharePoint.RepoService.RepoServiceClient"/> 
    /// or (TODO) with REST services
    /// </summary>
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = true)]
    [IisWebServiceApplicationBackupBehavior]
    [System.Runtime.InteropServices.Guid("ec3401d2-5109-4334-b88f-b9abe4e33e93")]
    internal sealed class RepoServiceApplication : SPIisWebServiceApplication
    {
        #region Scripts

        // currently active site url (TODO)
        private string _currentSiteUrl;

        // holds file contents for various site urls
        private Dictionary<string, Dictionary<string, FileAndDate>> _files = new Dictionary<string, Dictionary<string, FileAndDate>>();

        private Dictionary<string, Dictionary<string, FileAndDate>> Files
        {
            get
            {
                if (!_files.ContainsKey(_currentSiteUrl))
                {
                    _files[_currentSiteUrl] = LoadAllScripts(_currentSiteUrl);
                }

                return _files;
            }
        }

        private KeyValuePair<string, FileAndDate> LoadScriptFromItem(SPListItem tempitem)
        {
            return (KeyValuePair<string, FileAndDate>) tempitem.RunElevated(delegate(SPListItem item)
            {
                string relativePath =
                    RemoveMatchingStringFromStart(item.DocumentUrl(), item.ParentList.AbsoluteUrl()).Replace("/", "\\");

                KeyValuePair<string, FileAndDate> result = new KeyValuePair<string, FileAndDate>
                    (item.IsFolder() ? ".\\" + relativePath + "\\" : ".\\" + relativePath,
                        new FileAndDate(item.IsFolder() ? null : item.File.OpenBinaryStream().ReadAllBytes(),
                            (DateTime) item["Modified"]));
                return result;
            });
        }

        public void Reload(string siteUrl, string fileNamePath)
        {
            string totalUrl = null;

            try
            {
                using (SPSite tempsite = new SPSite(siteUrl))
                {
                    tempsite.RunElevated(delegate(SPSite site)
                    {
                        this._currentSiteUrl = siteUrl;

                        SPWeb rootWeb = site.RootWeb;
                        SPList sList = rootWeb.OpenList("SiteScripts", true);

                        // url seznamu spojit s dodanou cestou a jmenem souboru
                        totalUrl =
                            (sList.AbsoluteUrl() +
                             RepoServiceClient.NormalizedFilePath(fileNamePath).Replace('\\', '/').Substring(2)).Replace
                                ("/Forms/", "/");
                        SPListItem script = rootWeb.GetItemByUrl(totalUrl);

                        KeyValuePair<string, FileAndDate> scriptItem = LoadScriptFromItem(script);
                        _files[site.Url][scriptItem.Key] = scriptItem.Value;
                        return null;
                    });
                }
            }
            catch (FileNotFoundException f)
            {
                throw new FileNotFoundException("RepoService failed to open file " + ( totalUrl ?? "-" ) + "\n" + f);
            }
        }

        private string RemoveMatchingStringFromStart(string removeFrom, string toMatch)
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

        private Dictionary<string, FileAndDate> LoadAllScripts(string siteUrl)
        {
            if (siteUrl == null) throw new ArgumentNullException("siteUrl");

            var output = new Dictionary<string, FileAndDate>();

            using (SPSite tempsite = new SPSite(siteUrl))
            {
                return (Dictionary<string, FileAndDate>) tempsite.RunElevated(delegate(SPSite site)
                {
                    this._currentSiteUrl = site.Url;

                    SPWeb rootWeb = site.RootWeb;
                    SPList sList = rootWeb.OpenList("SiteScripts", true);

                    sList.ProcessItems(delegate(SPListItem script)
                    {
                        KeyValuePair<string, FileAndDate> scriptItem = LoadScriptFromItem(script);
                        output[scriptItem.Key] = scriptItem.Value;
                        return null;
                    });
                    return output;

                });
            }
        }

        public List<string> ListAllPaths(string siteUrl)
        {
            this._currentSiteUrl = siteUrl;

            List<string> fullpaths = new List<string> { "." };
            fullpaths.AddRange(Files[siteUrl].Keys);
            return fullpaths;
        }

        public Dictionary<string, string> GetScriptsWithExtension(Dictionary<string, string> scripts, string extension)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> kvp in scripts.Where(kvp => kvp.Key.ToLowerInvariant().EndsWith("." + extension)).Where(kvp => !output.ContainsKey(kvp.Key)))
            {
                output.Add(kvp.Key, kvp.Value);
            }

            return output;
        }

        public FileAndDate Get(string siteUrl, string siteScriptsFilePath)
        {
            if (string.IsNullOrWhiteSpace(siteScriptsFilePath)) throw new ArgumentNullException();

            this._currentSiteUrl = siteUrl;

            FileAndDate result = null;
            // if there's no directory in the filename, we need to prepend the root ./
            string scriptFilePathNormalized = RepoServiceClient.NormalizedFilePath(siteScriptsFilePath);
            if (Files[siteUrl].ContainsKey(scriptFilePathNormalized)) result = Files[siteUrl][scriptFilePathNormalized];
            return result;
        }

        public Dictionary<string, FileAndDate> GetAll(string siteUrl, IEnumerable<string> paths)
        {
            if (paths == null) throw new ArgumentNullException();

            this._currentSiteUrl = siteUrl;

            Dictionary<string, FileAndDate> fullscripts = new Dictionary<string, FileAndDate>();

            foreach (string path in paths)
            {
                string normalizedSubFolder = RepoServiceClient.NormalizedFilePath(path);
                foreach (string fnamepath in Files[siteUrl].Keys)
                {
                    if (fnamepath.StartsWith(normalizedSubFolder))
                    {
                        fullscripts[fnamepath] = Files[siteUrl][fnamepath];
                    }
                }
            }

            return fullscripts;
        }

        public void Reset(string siteUrl)
        {
            _files.Remove(siteUrl);
        }

        public Guid ServiceApplicationIisGuid()
        {
            return this.ApplicationPool.Id;
        }

        #endregion

        #region Plumbing

        #region Fields


        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RepoServiceApplication"/> class. Default constructor (required for SPPersistedObject serialization). Never call this directly.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public RepoServiceApplication() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="RepoServiceApplication"/> class. Use this constructor when creating a new Service Application (e.g. from code in your Create page)
        /// </summary>
        /// <param name="name">The name of the service application.</param>
        /// <param name="service">The <see cref="Navertica.SharePoint.RepoService.Service" />.</param>
        /// <param name="applicationPool">The application pool.</param>
        internal RepoServiceApplication(string name, RepoService service, SPIisWebServiceApplicationPool applicationPool)
            : base(name, service, applicationPool) {}

        #endregion

        #region Properties

        /// <summary>
        /// Gets the TypeName. This string will display in the Type column on the Manage Service Applications screen. You can localize this value. If you don't override this, 
        /// the default string in the Type column will be the name of this type from GetType().
        /// </summary>
        public override string TypeName
        {
            get { return SPUtility.GetLocalizedString("$Resources:ServiceApplicationName", "NVRRepoService.ServiceResources", (uint) System.Threading.Thread.CurrentThread.CurrentCulture.LCID); }
        }

        /// <summary>
        /// Gets the link to the Management page for this service application. Use this page to provide a UI for changing and configuring your application-specific settings.
        /// </summary>
        public override SPAdministrationLink ManageLink
        {
            get { return new SPAdministrationLink(string.Format(CultureInfo.InvariantCulture, "/_admin/NVRRepoService/ManageApplication.aspx?{0}", SPHttpUtility.UrlKeyValueEncode("id", this.Id.ToString()))); }
        }

        /// <summary>
        /// Gets the link to the Properties page for this service application. Use this page to enable the user to change basic settings such as the Application Pool.
        /// </summary>
        public override SPAdministrationLink PropertiesLink
        {
            get { return new SPAdministrationLink(string.Format(CultureInfo.InvariantCulture, "/_admin/NVRRepoService/Properties.aspx?{0}", SPHttpUtility.UrlKeyValueEncode("id", this.Id.ToString()))); }
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
            get { return new Guid("405e70c4-1eb3-4524-9711-27270e574813"); }
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
            get { return "NVRRepoService.svc"; }
        }

        /// <summary>
        /// Gets the install path to the subfolder of the WebServices folder.
        /// </summary>
        protected override string InstallPath
        {
            get { return SPUtility.GetVersionedGenericSetupPath(@"WebServices\NVRRepoService", 0); }
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