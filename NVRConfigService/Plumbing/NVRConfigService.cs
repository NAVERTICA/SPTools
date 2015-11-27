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

using Navertica.SharePoint.ConfigService.Service;

namespace Navertica.SharePoint.ConfigService.Administration
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Utilities;

    /// <summary>
    /// The Service. This is registered once per farm.
    /// </summary>
    [System.Runtime.InteropServices.Guid("ab79af9f-3291-4490-ad68-7bfc457b0368")]
    internal sealed class NVRConfigService : SPIisWebService, IServiceAdministration
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NVRConfigService"/> class. Default constructor (required for SPPersistedObject serialization). Never call this directly.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public NVRConfigService()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NVRConfigService"/> class. Use this constructor to install the service in the farm if it doesn't exist.
        /// </summary>
        /// <param name="farm">The <see cref="Microsoft.SharePoint.Administration.SPFarm"/> that this service will be installed in.</param>
        internal NVRConfigService(SPFarm farm)
            : base(farm)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the service. This will display on the Services on Server screen. You can localize this value.
        /// </summary>
        public override string TypeName
        {
            get
            {
                return SPUtility.GetLocalizedString("$Resources:ServiceName", "NVRConfigService.ServiceResources", (uint)System.Threading.Thread.CurrentThread.CurrentCulture.LCID);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the link to an ASPX page that can be used to create new Service Applications. Without this, you will not be able to create new service applications on the Manage Service Applications screen.
        /// </summary>
        /// <param name="serviceApplicationType">The <see cref="System.Type" /> of service application.</param>
        /// <returns>A <see cref="Microsoft.SharePoint.Administration.SPAdministrationLink" />.</returns>
        public override SPAdministrationLink GetCreateApplicationLink(Type serviceApplicationType)
        {
            if (serviceApplicationType != typeof(ConfigServiceApplication))
            {
                throw new NotSupportedException();
            }

            return new SPAdministrationLink("/_admin/NVRConfigService/CreateApplication.aspx");
        }

        /// <summary>
        /// Gets the options for creating a new service application. Used to explicitly determine how/if the Farm Configuration Wizard can be used to provision this service application.
        /// </summary>
        /// <param name="serviceApplicationType">The <see cref="System.Type" /> of service application.</param>
        /// <returns>An <see cref="Microsoft.SharePoint.Administration.SPCreateApplicationOptions" /> value.</returns>
        public override SPCreateApplicationOptions GetCreateApplicationOptions(Type serviceApplicationType)
        {
            if (serviceApplicationType != typeof(ConfigServiceApplication))
            {
                throw new NotSupportedException();
            }

            return SPCreateApplicationOptions.None;
        }

        /// <summary>
        /// Used for the Farm Configuration Wizard. Create the service application programmatically here if you want to support Single-Click install.
        /// </summary>
        /// <param name="name">The name of the service application.</param>
        /// <param name="serviceApplicationType">The <see cref="System.Type" /> of service application.</param>
        /// <param name="provisioningContext">The SPServiceProvisioningContext (will be passed in by the farm configuration wizard).</param>
        /// <returns>A reference to a ServiceApplication.</returns>
        public SPServiceApplication CreateApplication(string name, Type serviceApplicationType, SPServiceProvisioningContext provisioningContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Used for the Farm Configuration Wizard. Create the service application proxy programmatically here if you want to support Single-Click install.
        /// </summary>
        /// <param name="name">The name of the Service Application Proxy.</param>
        /// <param name="serviceApplication">The Service Application to associate with this proxy (will be passed in automatically by the configuration wizard).</param>
        /// <param name="provisioningContext">The SPServiceProvisioningContext (will be passed in by the farm configuration wizard).</param>
        /// <returns>A reference to a ServiceApplicationProxy.</returns>
        public SPServiceApplicationProxy CreateProxy(string name, SPServiceApplication serviceApplication, SPServiceProvisioningContext provisioningContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a friendly name and description for this service application for display in the farm configuration wizard. You can localize these values.
        /// </summary>
        /// <param name="serviceApplicationType">The <see cref="System.Type" /> of service application.</param>
        /// <returns>An <see cref="SPPersistedTypeDescription"/> containing the name and description of the service application.</returns>
        public SPPersistedTypeDescription GetApplicationTypeDescription(Type serviceApplicationType)
        {
            if (serviceApplicationType != typeof(ConfigServiceApplication))
            {
                throw new NotSupportedException();
            }

            return new SPPersistedTypeDescription(
                SPUtility.GetLocalizedString("$Resources:ServiceApplicationName", "NVRConfigService.ServiceResources", (uint)System.Threading.Thread.CurrentThread.CurrentCulture.LCID),
                SPUtility.GetLocalizedString("$Resources:ServiceApplicationDescription", "NVRConfigService.ServiceResources", (uint)System.Threading.Thread.CurrentThread.CurrentCulture.LCID));
        }

        /// <summary>
        /// Gets an array of the service application types supported by the service.
        /// </summary>
        /// <returns>An array of supported service application types.</returns>
        public Type[] GetApplicationTypes()
        {
            return new Type[] { typeof(ConfigServiceApplication) };
        }

        /// <summary>
        /// Gets an existing service or creates it if it doesn't exist.
        /// </summary>
        /// <returns>An instance of the Service.</returns>
        internal static NVRConfigService GetOrCreateService()
        {
            NVRConfigService service = SPFarm.Local.Services.GetValue<NVRConfigService>();
            if (service == null)
            {
                service = new NVRConfigService(SPFarm.Local);
                service.Status = SPObjectStatus.Online;
                service.Update();
            }

            return service;
        }

        /// <summary>
        /// Removes the service and components from the farm.
        /// </summary>
        internal static void RemoveService()
        {
            NVRConfigService service = SPFarm.Local.Services.GetValue<NVRConfigService>();
            ConfigServiceProxy serviceProxy = SPFarm.Local.ServiceProxies.GetValue<ConfigServiceProxy>();

            // Uninstall any service applications          
            if (service != null)
            {
                foreach (SPServiceApplication app in service.Applications)
                {
                    app.Unprovision(true);
                    app.Delete();
                }
            }

            // Uninstall any remaining service application proxies (e.g. any connections to other farms)          
            if (serviceProxy != null)
            {
                foreach (SPServiceApplicationProxy proxy in serviceProxy.ApplicationProxies)
                {
                    proxy.Unprovision(true);
                    proxy.Delete();
                }
            }

            // Uninstall the instances
            foreach (SPServer server in SPFarm.Local.Servers)
            {
                ConfigServiceInstance serviceInstance = server.ServiceInstances.GetValue<ConfigServiceInstance>();
                while (serviceInstance != null)
                {
                    server.ServiceInstances.Remove(serviceInstance.Id);
                    serviceInstance = server.ServiceInstances.GetValue<ConfigServiceInstance>();
                }
            }

            // Uninstall the service proxy
            if (serviceProxy != null)
            {
                SPFarm.Local.ServiceProxies.Remove(serviceProxy.Id);
            }

            // Uninstall the service
            if (service != null)
            {
                SPFarm.Local.Services.Remove(service.Id);
            }
        }

        #endregion
    }
}
