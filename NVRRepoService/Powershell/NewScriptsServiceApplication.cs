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

namespace Navertica.SharePoint.RepoService.PowerShell
{
    using System;
    using System.Management.Automation;
    using System.Net;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.PowerShell;
    using Service;

    /// <summary>
    /// Creates a new service application.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "RepoServiceApplication", SupportsShouldProcess = true, DefaultParameterSetName = "DefaultSet")]
    [SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    [System.Runtime.InteropServices.Guid("a1f038e5-1bc2-436d-a346-71ff50c2507a")]
    public sealed class NewScriptsServiceApplication : SPCmdlet
    {
        #region Fields

        /// <summary>
        /// The application pool.
        /// </summary>
        private SPIisWebServiceApplicationPoolPipeBind applicationPool;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the service application.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;

            set;
        }

        /// <summary>
        /// Gets or sets the application pool to use for the service application.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true)]
        [ValidateNotNull]
        public SPIisWebServiceApplicationPoolPipeBind ApplicationPool
        {
            get
            {
                return this.applicationPool;
            }

            set
            {
                this.applicationPool = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// This method gets invoked when the command is called
        /// </summary>
        protected override void InternalProcessRecord()
        {
            SPIisWebServiceApplicationPool resolvedApplicationPool = this.ApplicationPool.Read();

            if (resolvedApplicationPool == null)
            {
                this.ThrowTerminatingError(new InvalidOperationException("Could not find the specified application pool."), ErrorCategory.InvalidOperation, this);
            }

            if (this.ShouldProcess(this.Name))
            {
                // Get or create the service
                RepoService service = RepoService.GetOrCreateService();

                // Get or create the service proxy
                RepoServiceProxy.GetOrCreateServiceProxy();

                // Install the service instances to servers in this farm
                RepoServiceInstance.CreateServiceInstances(service);

                // Create the service application
                RepoServiceApplication application = new RepoServiceApplication(this.Name, service, resolvedApplicationPool);
                application.Update();
                application.Provision();

                this.WriteObject(application);
            }
        }

        /// <summary>
        /// Validate the arguments.
        /// </summary>
        protected override void InternalValidate()
        {
            // Validate a farm exists
            SPFarm farm = SPFarm.Local;
            if (farm == null)
            {
                this.ThrowTerminatingError(new InvalidOperationException("SharePoint server farm not found."), ErrorCategory.InvalidOperation, this);
            }

            SPServer server = SPServer.Local;
            if (server == null)
            {
                this.ThrowTerminatingError(new InvalidOperationException("SharePoint local server not found."), ErrorCategory.InvalidOperation, this);
            }

            // Get the service
            RepoService service = SPFarm.Local.Services.GetValue<RepoService>();

            if (service != null)
            {
                // Check for duplicate name
                SPServiceApplication application = service.Applications[this.Name];

                if (application != null)
                {
                    this.ThrowTerminatingError(new InvalidOperationException("A service application with that name already exists."), ErrorCategory.InvalidOperation, this);
                }
            }
        }

        #endregion
    }
}
