using System;
using System.Collections.Generic;
using System.Security.Policy;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;
using Navertica.SharePoint.RepoService;

namespace Navertica.SharePoint.RepoService.Service
{
    /// <summary>
    /// The WCF Service. Handing through the calls.
    /// </summary>
    [System.Runtime.InteropServices.Guid("43a59e27-5e6b-4e1b-b630-17021d917b8e")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated by the WCF runtime automatically.")]
    internal class RepoWCFService : IRepoWCFService
    {
        #region Methods

        public void Reload(string siteUrl, string filenamepath)
        {
            RepoServiceApplication current = (RepoServiceApplication) SPIisWebServiceApplication.Current;
            current.Reload(siteUrl, filenamepath);
        }

        public FileAndDate Get(string siteUrl, string filenamepath)
        {
            RepoServiceApplication current = (RepoServiceApplication) SPIisWebServiceApplication.Current;
            return current.Get(siteUrl, filenamepath);
        }

        public List<string> ListAllPaths(string siteUrl)
        {
            RepoServiceApplication current = (RepoServiceApplication) SPIisWebServiceApplication.Current;
            return current.ListAllPaths(siteUrl);
        }

        /// <summary>
        /// Returns dict of filenames and key-value pairs with date of last change and script body.
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="paths">files and paths in SiteScripts</param>
        /// <returns>A string of contents of the file.</returns>
        public Dictionary<string, FileAndDate> GetAll(string siteUrl, IEnumerable<string> paths)
        {
            RepoServiceApplication current = (RepoServiceApplication) SPIisWebServiceApplication.Current;
            return current.GetAll(siteUrl, paths);
        }

        public string NormalizedFilePath(string original)
        {
            return RepoServiceClient.NormalizedFilePath(original);
        }

        public void Reset(string siteUrl)
        {
            RepoServiceApplication current = (RepoServiceApplication) SPIisWebServiceApplication.Current;
            current.Reset(siteUrl);
        }

        public Guid ServiceApplicationIisGuid()
        {
            RepoServiceApplication current = (RepoServiceApplication) SPIisWebServiceApplication.Current;
            return current.ServiceApplicationIisGuid();
        }

        #endregion
    }
}