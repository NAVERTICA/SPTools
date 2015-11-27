using System;
using System.Runtime.InteropServices;
using Microsoft.Practices.ServiceLocation;
using Microsoft.SharePoint;
using Navertica.SharePoint.Interfaces;
using Navertica.SharePoint.Logging;
using Microsoft.Practices.SharePoint.Common.ServiceLocation;
using Navertica.SharePoint.PageHead;

namespace Navertica.SharePoint.Features
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("5e0e96d3-4c3a-4723-9d3c-90af61b7391b")]
    public class NVRPageHeadTranslationsEventReceiver : SPFeatureReceiver
    {
        private ILogging log = NaverticaLog.GetInstance();

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            try
            {
                IServiceLocator serviceLocator = SharePointServiceLocator.GetCurrent();
                IServiceLocatorConfig typeMappings = serviceLocator.GetInstance<IServiceLocatorConfig>();
                typeMappings.Site = properties.Feature.Parent as SPSite;
                typeMappings.RegisterTypeMapping<IPageHead, TranslationsHead>("Translations");
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
                typeMappings.RemoveTypeMapping<IPageHead>("Translations");
                SharePointServiceLocator.Reset();
            }
            catch (Exception exc)
            {
                log.LogException(exc);
            }
        }
    }
}