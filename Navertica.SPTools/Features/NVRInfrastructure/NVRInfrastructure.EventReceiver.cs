using System;
using System.Runtime.InteropServices;
using Microsoft.SharePoint;
using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.Extensions;
using Navertica.SharePoint.Infrastructure;
using Navertica.SharePoint.RepoService;
using System.Web;

namespace Navertica.SharePoint.Features
{
    [Guid("bfa0de52-3aa1-432f-a88a-eb4c1452bacb")]
    public class NVRInfrastructureEventReceiver : NVRFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            // copy files and items via NVRFeatureReceiver
            base.FeatureActivated(properties);
            SPSite site = properties.Feature.Parent as SPSite;

            if (site == null) throw new NullReferenceException("Couldn't load parent site");

            // reset our internal list name cache - will be done by SPListEventReceivers in the future
            HttpRuntime.Cache.Remove("OpenlistDict_" + site.ID + site.RootWeb.ID);

            bool firstTimeInstallSiteConfig = false;
            bool firstTimeInstallSiteScripts = false;
            if (site.RootWeb.OpenList(Const.SITECONFIG_LIST_TITLE, false) == null) firstTimeInstallSiteConfig = true;
            if (site.RootWeb.OpenList(Const.SITESCRIPTS_LIST_TITLE, false) == null) firstTimeInstallSiteScripts = true;


            log.LogInfo(properties.Feature.Definition.DisplayName + " feature receiver starting");

            try
            {
                // Deploy SiteScripts library and SiteConfig list
                properties.Definition.Farm.Properties["NVR_ScriptsServiceBaseUrl"] = site.Url;
                SiteStructuresDefinitions.DeploySiteModel(site);
                SiteStructuresDefinitions.DeployRootWebStructures(site);
            }
            catch (Exception exc)
            {
                log.LogException(exc, "SPMeta2 deploying models");
            }

            try
            {
                site.RootWeb.EventReceivers.Add(SPEventReceiverType.ListAdded, Const.NVR_DLL, Const.NVR_LIST_RECEIVER);
                site.RootWeb.EventReceivers.Add(SPEventReceiverType.ListAdding, Const.NVR_DLL, Const.NVR_LIST_RECEIVER);
                site.RootWeb.EventReceivers.Add(SPEventReceiverType.ListDeleted, Const.NVR_DLL, Const.NVR_LIST_RECEIVER);
                site.RootWeb.EventReceivers.Add(SPEventReceiverType.ListDeleting, Const.NVR_DLL, Const.NVR_LIST_RECEIVER);
                site.RootWeb.EventReceivers.Add(SPEventReceiverType.FieldAdded, Const.NVR_DLL, Const.NVR_LIST_RECEIVER);
                site.RootWeb.EventReceivers.Add(SPEventReceiverType.FieldAdding, Const.NVR_DLL, Const.NVR_LIST_RECEIVER);
                site.RootWeb.EventReceivers.Add(SPEventReceiverType.FieldDeleted, Const.NVR_DLL, Const.NVR_LIST_RECEIVER);
                site.RootWeb.EventReceivers.Add(SPEventReceiverType.FieldDeleting, Const.NVR_DLL, Const.NVR_LIST_RECEIVER);
                site.RootWeb.EventReceivers.Add(SPEventReceiverType.FieldUpdated, Const.NVR_DLL, Const.NVR_LIST_RECEIVER);
                site.RootWeb.EventReceivers.Add(SPEventReceiverType.FieldUpdating, Const.NVR_DLL, Const.NVR_LIST_RECEIVER);
            }
            catch (Exception exc)
            {
                log.LogException(exc, "adding SPListEventReceivers to root web");
                throw;
            }

            // reset our internal list name cache - will be done by SPListEventReceivers in the future
            HttpRuntime.Cache.Remove("OpenlistDict_" + site.ID + site.RootWeb.ID);

            // REMOVE ACCESS 
            if (firstTimeInstallSiteConfig)
            {
                try
                {
                    SPList SiteConfig = site.RootWeb.OpenList(Const.SITECONFIG_LIST_TITLE, false);
                    if (SiteConfig != null)
                    {
                        SiteConfig.RemoveRights();
                    }
                }
                catch (Exception exc)
                {
                    log.LogException(exc, "removing rights from SiteConfig");
                    throw;
                }
            }

            if (firstTimeInstallSiteScripts)
            {
                try
                {
                    SPList SiteScripts = site.RootWeb.OpenList(Const.SITESCRIPTS_LIST_TITLE, false);
                    if (SiteScripts != null)
                    {
                        SiteScripts.RemoveRights();
                    }
                }
                catch (Exception exc)
                {
                    log.LogException(exc, "removing rights from SiteScripts");
                    throw;
                }
            }

            try
            {
                ConfigServiceClient cfg = new ConfigServiceClient(site);
                RepoServiceClient repo = new RepoServiceClient(site);
                cfg.Reset(site.Url);
                repo.Reset();
                PluginHost.Reset();
            }
            catch (Exception e)
            {
                log.LogException(e);
                throw;
            }

            log.LogInfo(properties.Feature.Definition.DisplayName + " feature receiver finished");
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