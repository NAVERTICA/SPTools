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

    [Guid("f91df6c3-faa1-4ab8-9b28-2026528c1aa8")]
    public class NVRPageHeadRibbonEventReceiver : SPFeatureReceiver
    {
        private ILogging log = NaverticaLog.GetInstance();

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            try
            {
                IServiceLocator serviceLocator = SharePointServiceLocator.GetCurrent();
                IServiceLocatorConfig typeMappings = serviceLocator.GetInstance<IServiceLocatorConfig>();
                typeMappings.Site = properties.Feature.Parent as SPSite;
                typeMappings.RegisterTypeMapping<PageHead.IPageHead, PageHead.RibbonHead>("Ribbon");
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
                typeMappings.RemoveTypeMapping<PageHead.IPageHead>("Ribbon");
                SharePointServiceLocator.Reset();
            }
            catch (Exception exc)
            {
                log.LogException(exc);
            }
        }
    }
}