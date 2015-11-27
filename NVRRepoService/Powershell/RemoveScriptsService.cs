//-----------------------------------------------------------------------
// <copyright file="RemoveScriptsService.cs" company="">
// Copyright © 
// </copyright>
//-----------------------------------------------------------------------

using Navertica.SharePoint.RepoService.Service;

namespace Navertica.SharePoint.RepoService.PowerShell
{
    using System;
    using System.Management.Automation;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.PowerShell;

    /// <summary>
    /// Remove the service from the farm.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "RepoServiceApplication", SupportsShouldProcess = true)]
    [SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    [System.Runtime.InteropServices.Guid("cd11273f-1df1-4d1b-851e-d7cca87bb480")]
    public class RemoveScriptsService : SPCmdlet
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
                Service.RepoService.RemoveService();
            }
        }
    }
}
