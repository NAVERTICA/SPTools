﻿/*  Copyright (C) 2015 NAVERTICA a.s. http://www.navertica.com 

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

using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.SharePoint.Common.ServiceLocation;
using Navertica.SharePoint.Interfaces;


namespace Navertica.SharePoint.RepoService.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;

    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>
    [System.Runtime.InteropServices.Guid("4047e652-ed5a-4222-a9ee-3f26e45d8740")]
    public class ScriptsServiceAdministrationEventReceiver : SPFeatureReceiver
    {
        /// <summary>
        /// Feature installed event.
        /// </summary>
        /// <param name="properties">The feature properties.</param>
        public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        {
            try
            {
                // Ensure that the resource files in CONFIG\ADMINRESOURCES are copied to App_GlobalResources.
                // If you have Central Administration on another server, you will need to run 
                // stsadm -o copyappbincontent or Install-SPApplicationContent on that server directly, 
                // as the call below only runs on the server that the WSP is deployed on.
                SPWebService.AdministrationService.ApplyApplicationContentToLocalServer();
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Service Applications", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
                throw;
            }

          
        }

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            //Pro namapovani enginu mame zvlast projekt a features
            /*
            try
            {
                IServiceLocator serviceLocator = SharePointServiceLocator.GetCurrent();
                IServiceLocatorConfig serviceLocatorConfig = serviceLocator.GetInstance<IServiceLocatorConfig>();
                serviceLocatorConfig.RegisterTypeMapping<IExecuteScript, DLRExecute>();
                SharePointServiceLocator.Reset();
                
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Administration", TraceSeverity.High, EventSeverity.Information), TraceSeverity.High, "Registered IExecuteScript type mapping - current count " + serviceLocator.GetAllInstances<IExecuteScript>().Count().ToString());
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Service Applications", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
            }
            base.FeatureActivated(properties);
           */

        }
    }
}
