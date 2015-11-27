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