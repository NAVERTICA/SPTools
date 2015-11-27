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