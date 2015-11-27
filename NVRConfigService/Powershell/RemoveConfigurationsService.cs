//-----------------------------------------------------------------------
// <copyright file="RemoveConfigService.cs" company="">
// Copyright © 
// </copyright>
//-----------------------------------------------------------------------

using Navertica.SharePoint.ConfigService.Administration;

namespace Navertica.SharePoint.ConfigService.PowerShell
{
    using System;
    using System.Management.Automation;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.PowerShell;

    /// <summary>
    /// Remove the service from the farm.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "ConfigServiceApplication", SupportsShouldProcess = true)]
    [SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    [System.Runtime.InteropServices.Guid("b1cddf19-a022-4003-a68b-0e93c7016a2e")]
    public class RemoveConfigService : SPCmdlet
    {
        /// <summary>
        /// Gets or sets whether to delete any data associated with any service applications.
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        [ValidateNotNullOrEmpty]
        public SwitchParameter DeleteData
        {
            get;

            set;
        }

        /// <summary>
        /// This method gets invoked when the command is called
        /// </summary>
        protected override void InternalProcessRecord()
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

            if (this.ShouldProcess(SPFarm.Local.Name))
            {
                // Remove the service
                NVRConfigService.RemoveService();
            }
        }
    }
}
