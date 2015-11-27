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

using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Navertica.SharePoint.ConfigService.Service
{
    using System;
    using System.ServiceModel;
    using System.Collections.Generic;

    /// <summary>
    /// The Service Contract - Interface of the WCF service.
    /// </summary>
    [ServiceContract]
    [System.Runtime.InteropServices.Guid("079828d3-e9c4-4b8a-8049-14083e90cd1a")]
    internal interface IConfigurationsWCFService
    {
        #region Methods

        [OperationContract]
        void Reload(string siteWebUrl, string relativeItemUrl);

        [OperationContract]
        ConfigGuidList Filter(string siteWebUrl, Dictionary<ConfigFilterType, string> filterSettings, string forUserLogin);

        [OperationContract]
        string GetBranchJson(Guid itemId, string name);

        [OperationContract]
        void Reset(string siteWebUrl);

        [OperationContract]
        Guid ServiceApplicationIisGuid();

        #endregion
    }
}