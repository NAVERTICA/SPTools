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
    /// Updates the service application.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "RepoServiceApplication", SupportsShouldProcess = true)]
    [SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    [System.Runtime.InteropServices.Guid("1b21a562-dc68-43dc-a001-0c806979049e")]
    public class SetScriptsServiceApplication : SPCmdlet
    {
        #region Fields
        /// <summary>
        /// The application pool.
        /// </summary>
        private SPServiceApplicationPipeBind application;

        /// <summary>
        /// The name of the service application.
        /// </summary>
        private string name;

        /// <summary>
        /// An application pool to update
        /// </summary>
        private SPIisWebServiceApplicationPoolPipeBind applicationPool;

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the service application to update.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public SPServiceApplicationPipeBind Identity
        {
            get
            {
                return this.application;
            }

            set
            {
                this.application = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
            }
        }

        /// <summary>
        /// Gets or sets the application pool.
        /// </summary>
        [Parameter(Mandatory = false)]
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
            SPServiceApplication resolvedApplication = null;
            RepoServiceApplication castedApplication = null;

            resolvedApplication = this.Identity.Read();

            if (resolvedApplication == null)
            {
                this.ThrowTerminatingError(new InvalidOperationException("No service application was found."), ErrorCategory.InvalidOperation, this);
            }

            castedApplication = resolvedApplication as RepoServiceApplication;

            if (castedApplication == null)
            {
                this.ThrowTerminatingError(new InvalidOperationException("The service application provided was not of the correct type."), ErrorCategory.InvalidOperation, this);
            }

            if (this.ShouldProcess(castedApplication.Name))
            {
                // Update the name
                if (!string.IsNullOrEmpty(this.Name) && (!string.Equals(this.Name.Trim(), castedApplication.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    // Get the service
                    RepoService service = SPFarm.Local.Services.GetValue<RepoService>();

                    if (service != null)
                    {
                        // Check for duplicate name
                        SPServiceApplication duplicateApplication = service.Applications[this.Name.Trim()];

                        if (duplicateApplication != null)
                        {
                            this.ThrowTerminatingError(new InvalidOperationException("A service application with that name already exists."), ErrorCategory.InvalidOperation, this);
                        }
                    }

                    castedApplication.Name = this.Name.Trim();
                }

                // Update the application pool
                if (this.ApplicationPool != null)
                {
                    SPIisWebServiceApplicationPool resolvedApplicationPool = this.ApplicationPool.Read();

                    if (resolvedApplicationPool != null)
                    {
                        castedApplication.ApplicationPool = resolvedApplicationPool;
                    }
                }

                castedApplication.Update();
            }
        }

        /// <summary>
        /// Validates the arguments.
        /// </summary>
        protected override void InternalValidate()
        {
            // Validate a farm exists
            SPFarm farm = SPFarm.Local;
            if (farm == null)
            {
                this.ThrowTerminatingError(new InvalidOperationException("SharePoint server farm not found."), ErrorCategory.ResourceUnavailable, this);
            }

            SPServer server = SPServer.Local;
            if (server == null)
            {
                this.ThrowTerminatingError(new InvalidOperationException("SharePoint local server not found."), ErrorCategory.ResourceUnavailable, this);
            }
        }

        #endregion
    }
}
