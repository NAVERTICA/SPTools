using System;
using System.Runtime.InteropServices;
using Microsoft.SharePoint;

namespace Navertica.SharePoint.Features
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("d41be5af-7903-4137-b7bb-afeae480574a")]
    public class InstallPluginsEventReceiver : NVRFeatureReceiver
    {
        // Uncomment the method below to handle the event raised after a feature has been activated.
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            try
            {
                SPSite site = properties.Feature.Parent as SPSite;
                if (site == null) throw new NullReferenceException("Couldn't load parent site");

                // Scripts Service config properties
                properties.Definition.Farm.Properties["NVR_ScriptsServiceBaseUrl"] = site.Url;

                CopySiteScriptsFilesToLibrary(site, properties.Definition.Properties["NVR_SiteScriptsFolder"].Value); //from git, proted z konzole
                CopySiteConfigItems(site, properties.Definition.Properties["NVR_SiteConfigFolder"].Value, "NVR_SiteConfigJSON");
            }
            catch (Exception e)
            {
                log.LogException("exception in feature receiver of " + properties.Definition.DisplayName, e);
            }

        }

        // Uncomment the method below to handle the event raised before a feature is deactivated.

        //public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised after a feature has been installed.

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}
    }
}