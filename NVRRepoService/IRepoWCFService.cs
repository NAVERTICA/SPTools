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
using System.ServiceModel;
using Navertica.SharePoint.RepoService;

namespace Navertica.SharePoint.RepoService.Service
{
    /// <summary>
    /// The Service Contract - Interface of the WCF service.
    /// </summary>
    [ServiceContract]
    [System.Runtime.InteropServices.Guid("2fa28276-f89e-4d1a-95f3-6609dcfd22b5")]
    internal interface IRepoWCFService
    {
        #region Methods

        /// <summary>
        /// Tells the service to load/reload current contents of the file
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="filenamepath">path to file with root set as the root folder of the SiteScripts library
        /// - "/rootscript.py" or "/nested/in/folders/script.js"</param>        
        [OperationContract]
        void Reload(string siteUrl, string filenamepath);

        /// <summary>
        /// Get contents and last modification date of specified file
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="filenamepath">path and file (root is in SiteScripts library root)</param>
        /// <returns>Return two values - the Key is date of last modification, Value is the script content itself - or null, if file doesn't exist</returns>
        [OperationContract]
        FileAndDate Get(string siteUrl, string filenamepath);

        /// <summary>
        /// Returns list of full paths to all files and directories in SiteScripts 
        /// </summary>                
        [OperationContract]
        List<string> ListAllPaths(string siteUrl);

        /// <summary>
        /// Returns dict of filenames and key-value pairs with date of last change and file contents.
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="subpaths">list of all the paths that should be included (relative to SiteScripts)</param>
        /// <returns>A string of contents of the file.</returns>
        [OperationContract]
        Dictionary<string, FileAndDate> GetAll(string siteUrl, IEnumerable<string> subpaths);

        /// <summary>
        /// Normalize path to script to the format used in Scripts Service App
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        [OperationContract]
        string NormalizedFilePath(string original);

        /// <summary>
        /// Drop and reload all script data
        /// </summary>
        [OperationContract]
        void Reset(string siteUrl);

        /// <summary>
        /// Recycle ApplicationPool of service
        /// </summary>
        [OperationContract]
        Guid ServiceApplicationIisGuid();

        #endregion
    }
}