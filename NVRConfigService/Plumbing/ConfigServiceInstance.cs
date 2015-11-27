//-----------------------------------------------------------------------
// <copyright file="ConfigServiceInstance.cs" company="">
// Copyright © 
// </copyright>
//-----------------------------------------------------------------------

using Navertica.SharePoint.ConfigService.Administration;

namespace Navertica.SharePoint.ConfigService.Service
{
    using System;
    using System.ComponentModel;
    using Microsoft.SharePoint.Administration;

    /// <summary>
    /// The service instance. Appears on the Services on Server screen in SharePoint Central Administration. There can be 
    /// one service instance per server on the farm. Administrators can stop/start the service on individual servers. Each 
    /// server that the service is started on will participate in the automatic load-balancing in the service application proxy.
    /// </summary>
    [System.Runtime.InteropServices.Guid("86a0b036-0747-4f49-8299-57934cdb13f0")]
    internal sealed class ConfigServiceInstance : SPIisWebServiceInstance
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigServiceInstance"/> class. Default constructor (required for SPPersistedObject serialization). Never call this directly.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ConfigServiceInstance()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigServiceInstance"/> class. Use this constructor to install the service instance on servers in the farm.
        /// </summary>
        /// <param name="server">The SPServer to install the instance to.</param>
        /// <param name="service">The service to associate the service instance with.</param>
        internal ConfigServiceInstance(SPServer server, NVRConfigService service)
            : base(server, service)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the management link. This property makes the Service Instance a clickable hyperlink on the Services on Server page.
        /// </summary>
        public override SPActionLink ManageLink
        {
            get
            {
                return new SPActionLink("/_admin/NVRConfigService/ManageService.aspx");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Installs the service instances on servers in the farm (does not start them).
        /// </summary>
        /// <param name="service">The service associated with these instances.</param>
        internal static void CreateServiceInstances(NVRConfigService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            foreach (SPServer server in SPFarm.Local.Servers)
            {
                if (server.Role == SPServerRole.Application || server.Role == SPServerRole.SingleServer || server.Role == SPServerRole.WebFrontEnd)
                {
                    ConfigServiceInstance instance = server.ServiceInstances.GetValue<ConfigServiceInstance>();
                    if (instance == null)
                    {
                        instance = new ConfigServiceInstance(server, service);
                        instance.Update();
                    }
                }
            }
        }

        #endregion
    }
}