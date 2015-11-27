//-----------------------------------------------------------------------
// <copyright file="RepoServiceHostFactory.cs" company="">
// Copyright © 
// </copyright>
//-----------------------------------------------------------------------

namespace Navertica.SharePoint.RepoService.Service
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using Microsoft.SharePoint;

    /// <summary>
    /// This class enables the WCF services to support claims authentication.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated by the WCF Runtime automatically.")]
    [System.Runtime.InteropServices.Guid("e86832ad-c986-4f6f-866d-43f854cedf70")]
    internal sealed class RepoServiceHostFactory : ServiceHostFactory
    {
        /// <summary>
        /// Creates a service host.
        /// </summary>
        /// <param name="constructorString">The constructor string.</param>
        /// <param name="baseAddresses">The base url address.</param>
        /// <returns>A <see cref="ServiceHostBase"/>.</returns>
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            ServiceHostBase serviceHost = base.CreateServiceHost(constructorString, baseAddresses);
            serviceHost.Configure(SPServiceAuthenticationMode.Claims);
            return serviceHost;
        }
    }
}