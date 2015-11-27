//-----------------------------------------------------------------------
// <copyright file="HelloWorldWCFService.cs" company="">
// Copyright © 
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration.Claims;

namespace Navertica.SharePoint.ConfigService.Service
{
    using System;
    using System.ServiceModel;
    using Microsoft.SharePoint.Administration;

    /// <summary>
    /// The WCF Service. Handing through the calls.
    /// </summary>
    [System.Runtime.InteropServices.Guid("ad6cf79e-312a-4424-a62d-7ba63f82099f")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated by the WCF runtime automatically.")]
    internal class ConfigurationsWCFService : IConfigurationsWCFService
    {
        #region Methods

        public void Reload(string siteWebUrl, string relativeItemUrl)
        {
            var current = (ConfigServiceApplication)SPIisWebServiceApplication.Current;
            current.ReloadItem(siteWebUrl, relativeItemUrl);
        }

        public ConfigGuidList Filter(string siteWebUrl, Dictionary<ConfigFilterType, string> filterSettings, string forUserLogin)
        {
            var current = (ConfigServiceApplication)SPIisWebServiceApplication.Current;
            return current.Sort(current.Filter(current.AllConfigsForWeb(siteWebUrl), filterSettings, forUserLogin));
        }

        public string GetBranchJson(Guid itemId, string name)
        {
            var current = (ConfigServiceApplication)SPIisWebServiceApplication.Current;
            return current.GetBranchJson(itemId, name);
        }

        public void Reset(string siteWebUrl)
        {
            var current = (ConfigServiceApplication)SPIisWebServiceApplication.Current;
            current.Reset(siteWebUrl);            
        }

        public Guid ServiceApplicationIisGuid()
        {
            var current = (ConfigServiceApplication)SPIisWebServiceApplication.Current;
            return current.ServiceApplicationIisGuid();
        }

        #endregion 
    }
}
