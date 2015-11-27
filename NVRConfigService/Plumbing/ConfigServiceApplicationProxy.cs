//-----------------------------------------------------------------------
// <copyright file="ConfigServiceApplicationProxy.cs" company="">
// Copyright © 
// </copyright>
//-----------------------------------------------------------------------

namespace Navertica.SharePoint.ConfigService.Service
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Utilities;

    /// <summary>
    /// The Service Application Proxy. This contains the logic to provision the software load balancer.
    /// </summary>
    [System.Runtime.InteropServices.Guid("195eeec4-8063-4923-8398-1ebed57b9888")]
    [IisWebServiceApplicationProxyBackupBehavior]
    [SupportedServiceApplication("c0341fd5-43b5-4f2c-be52-4e2912a20a77", "1.0.0.0", typeof(ConfigServiceProxy))]
    internal sealed class ConfigServiceApplicationProxy : SPIisWebServiceApplicationProxy
    {
        #region Fields

        /// <summary>
        /// A reference to the load balancer.
        /// </summary>
        [Persisted]
        private SPServiceLoadBalancer loadBalancer;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigServiceApplicationProxy"/> class. Default constructor (required for SPPersistedObject serialization). Never call this directly.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ConfigServiceApplicationProxy()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigServiceApplicationProxy"/> class. Use this constructor to create a new Service Application Proxy (e.g. from code on the Create page).
        /// </summary>
        /// <param name="name">The name of the Service Application Proxy to create. This name will not be localized.</param>
        /// <param name="serviceProxy">A reference to the Service Proxy class.</param>
        /// <param name="serviceEndpointUri">The endpoint uri to the service.</param>
        internal ConfigServiceApplicationProxy(string name, ConfigServiceProxy serviceProxy, Uri serviceEndpointUri)
            : base(name, serviceProxy, serviceEndpointUri)
        {
            this.loadBalancer = new SPRoundRobinServiceLoadBalancer(serviceEndpointUri);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of this application proxy. 
        /// </summary>
        /// <remarks>
        /// This string will display in the Type column on the Manage Service Applications screen. 
        /// You can localize this value. If you don't override this, 
        /// the default string in the Type column will be the name of this type from GetType().
        /// </remarks>
        public override string TypeName
        {
            get
            {
                return SPUtility.GetLocalizedString("$Resources:ServiceApplicationProxyName", "NVRConfigService.ServiceResources", (uint)System.Threading.Thread.CurrentThread.CurrentCulture.LCID);
            }
        }

        /// <summary>
        /// Gets the WCF client configuration files for code that uses this Service Application Proxy.
        /// </summary>
        public System.Configuration.Configuration Configuration
        {
            get { return this.OpenClientConfiguration(SPUtility.GetVersionedGenericSetupPath(@"WebClients\NVRConfigService", 15)); }
        }

        /// <summary>
        /// Gets the software Load Balancer for code that uses this Service Application Proxy. 
        /// </summary>
        internal SPServiceLoadBalancer LoadBalancer
        {
            get
            {
                return this.loadBalancer;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Provision is called during installation of the Service Application Proxy (e.g. in code on the Create page).
        /// </summary>
        public override void Provision()
        {
            this.loadBalancer.Provision();
            base.Provision();
        }

        /// <summary>
        /// This method is called automatically when a service application that uses this proxy is deleted, or when the proxy itself is deleted.
        /// </summary>
        /// <param name="deleteData">True to delete data associated with this proxy.</param>
        public override void Unprovision(bool deleteData)
        {
            this.loadBalancer.Unprovision();
            base.Unprovision(deleteData);
        }

        #endregion
    }
}