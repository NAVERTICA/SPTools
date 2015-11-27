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

namespace Navertica.SharePoint.RepoService.Service
{
    using System;
    using System.ComponentModel;
    using Microsoft.SharePoint.Administration;

    /// <summary>
    /// The service instance. Appears on the Services on Server screen in SharePoint Central Administration. There can be 
    /// one service instance per server on the farm. Administrators can stop/start the service on individual servers. Each 
    /// server that the service is started on will participate in the automatic load-balancing in the service application proxy.
    /// </summary>
    [System.Runtime.InteropServices.Guid("1eb02f8d-5785-47c4-a801-7810eb7672db")]
    internal sealed class RepoServiceInstance : SPIisWebServiceInstance
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RepoServiceInstance"/> class. Default constructor (required for SPPersistedObject serialization). Never call this directly.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public RepoServiceInstance()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepoServiceInstance"/> class. Use this constructor to install the service instance on servers in the farm.
        /// </summary>
        /// <param name="server">The SPServer to install the instance to.</param>
        /// <param name="service">The service to associate the service instance with.</param>
        internal RepoServiceInstance(SPServer server, RepoService service)
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
                return new SPActionLink("/_admin/NVRRepoService/ManageService.aspx");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Installs the service instances on servers in the farm (does not start them).
        /// </summary>
        /// <param name="service">The service associated with these instances.</param>
        internal static void CreateServiceInstances(RepoService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            foreach (SPServer server in SPFarm.Local.Servers)
            {
                if (server.Role == SPServerRole.Application || server.Role == SPServerRole.SingleServer || server.Role == SPServerRole.WebFrontEnd)
                {
                    RepoServiceInstance instance = server.ServiceInstances.GetValue<RepoServiceInstance>();
                    if (instance == null)
                    {
                        instance = new RepoServiceInstance(server, service);
                        instance.Update();
                    }
                }
            }
        }

        #endregion
    }
}