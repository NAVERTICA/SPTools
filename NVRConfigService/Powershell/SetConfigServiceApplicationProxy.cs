//-----------------------------------------------------------------------
// <copyright file="SetConfigServiceApplicationProxy.cs" company="">
// Copyright © 
// </copyright>
//-----------------------------------------------------------------------

using Navertica.SharePoint.ConfigService.Service;

namespace Navertica.SharePoint.ConfigService.PowerShell
{
    using System;
    using System.Management.Automation;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.PowerShell;
    using ConfigService;

    /// <summary>
    /// Updates the service application proxy.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "ConfigServiceApplicationProxy", SupportsShouldProcess = true)]
    [SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    [System.Runtime.InteropServices.Guid("a6db3a82-b544-4d4e-96bd-27dcc65bcb5a")]
    public class SetConfigServiceApplicationProxy : SPCmdlet
    {
        #region Fields

        /// <summary>
        /// The service application.
        /// </summary>
        private SPServiceApplicationProxyPipeBind proxy;

        /// <summary>
        /// The name of the service application proxy.
        /// </summary>
        private string name;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the service application proxy to update.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public SPServiceApplicationProxyPipeBind Identity
        {
            get
            {
                return this.proxy;
            }

            set
            {
                this.proxy = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the proxy.
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
        /// Gets or sets whether the proxy is added to the default proxy group for the farm.
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter DefaultProxyGroup
        {
            get;

            set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// This method gets invoked when the command is called.
        /// </summary>
        protected override void InternalProcessRecord()
        {
            SPServiceApplicationProxy resolvedProxy = null;
            ConfigServiceApplicationProxy castedProxy = null;

            resolvedProxy = this.Identity.Read();

            if (resolvedProxy == null)
            {
                this.ThrowTerminatingError(new InvalidOperationException("No service application proxy was found."), ErrorCategory.InvalidOperation, this);
            }

            castedProxy = resolvedProxy as ConfigServiceApplicationProxy;

            if (castedProxy == null)
            {
                this.ThrowTerminatingError(new InvalidOperationException("The service application proxy was not of the correct type."), ErrorCategory.InvalidOperation, this);
            }

            if (this.ShouldProcess(castedProxy.Name))
            {
                if (!string.IsNullOrEmpty(this.Name) && (!string.Equals(this.Name.Trim(), castedProxy.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    // Get the service proxy and check for duplicate name
                    ConfigServiceProxy serviceProxy = SPFarm.Local.ServiceProxies.GetValue<ConfigServiceProxy>();
                    SPServiceApplicationProxy duplicateProxy = serviceProxy.ApplicationProxies[this.Name.Trim()];

                    if (duplicateProxy != null)
                    {
                        this.ThrowTerminatingError(new InvalidOperationException("There is already a service application proxy with that name."), ErrorCategory.InvalidOperation, this);
                    }

                    castedProxy.Name = this.Name.Trim();
                    castedProxy.Update();
                }

                if (this.DefaultProxyGroup.IsPresent)
                {
                    SPServiceApplicationProxyGroup group = SPServiceApplicationProxyGroup.Default;

                    if (this.DefaultProxyGroup.ToBool())
                    {
                        group.Add(castedProxy);
                    }
                    else
                    {
                        group.Remove(castedProxy.Id);
                    }
                }
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
