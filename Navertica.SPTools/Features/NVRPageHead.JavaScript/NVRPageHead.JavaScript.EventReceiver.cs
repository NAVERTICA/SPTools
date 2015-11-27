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
using System;
using System.Runtime.InteropServices;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.SharePoint.Common.ServiceLocation;
using Microsoft.SharePoint;
using Navertica.SharePoint.Interfaces;
using Navertica.SharePoint.Logging;

namespace Navertica.SharePoint.Features
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("c15ade31-927c-4200-b67e-65fcd14a7a17")]
    public class NVRPageHeadJavaScriptEventReceiver : SPFeatureReceiver
    {
        private ILogging log = NaverticaLog.GetInstance();

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            try
            {
                IServiceLocator serviceLocator = SharePointServiceLocator.GetCurrent();
                IServiceLocatorConfig typeMappings = serviceLocator.GetInstance<IServiceLocatorConfig>();
                typeMappings.Site = properties.Feature.Parent as SPSite;
                typeMappings.RegisterTypeMapping<Navertica.SharePoint.PageHead.IPageHead, Navertica.SharePoint.PageHead.JavaScriptHead>("JavaScript");
                SharePointServiceLocator.Reset();
            }
            catch (Exception exc)
            {
                log.LogException(exc);
            }
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            try
            {
                IServiceLocator serviceLocator = SharePointServiceLocator.GetCurrent();
                IServiceLocatorConfig typeMappings = serviceLocator.GetInstance<IServiceLocatorConfig>();
                typeMappings.Site = properties.Feature.Parent as SPSite;
                typeMappings.RemoveTypeMapping<Navertica.SharePoint.PageHead.IPageHead>("JavaScript");
                SharePointServiceLocator.Reset();
            }
            catch (Exception exc)
            {
                log.LogException(exc);
            }
        }
    }
}