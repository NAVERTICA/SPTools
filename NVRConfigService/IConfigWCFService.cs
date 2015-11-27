//-----------------------------------------------------------------------
// <copyright file="IHelloWorldWCFService.cs" company="">
// Copyright © 
// </copyright>
//-----------------------------------------------------------------------

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