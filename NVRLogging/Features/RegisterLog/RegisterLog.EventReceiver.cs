using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.SharePoint.Common.Configuration;
using Microsoft.Practices.SharePoint.Common.Logging;
using Microsoft.Practices.SharePoint.Common.ServiceLocation;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Security;
using Navertica.SharePoint.Interfaces;
using Navertica.SharePoint.Logging;

namespace Navertica.SharePoint.Features
{
    /// <summary>
    /// Create event log area and diagnostic categories
    /// </summary>
    [Guid("baa6eb49-9550-4bb0-9255-d4220af8068a")]
    public class RegisterLogEventReceiver : SPFeatureReceiver
    {
        private DiagnosticsAreaCollection _myAreas = null;

        private DiagnosticsAreaCollection MyAreas
        {
            get
            {
                if (_myAreas == null)
                {
                    _myAreas = new DiagnosticsAreaCollection();
                    DiagnosticsArea diagArea = new DiagnosticsArea(Constants.AREA_DEFAULT);

                    diagArea.DiagnosticsCategories.Add(new DiagnosticsCategory("Info", EventSeverity.Information, TraceSeverity.Medium));
                    diagArea.DiagnosticsCategories.Add(new DiagnosticsCategory("Warn", EventSeverity.Warning, TraceSeverity.Medium));
                    diagArea.DiagnosticsCategories.Add(new DiagnosticsCategory("Error", EventSeverity.Error, TraceSeverity.Medium));

                    _myAreas.Add(diagArea);
                }
                return _myAreas;
            }
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/ff647661.aspx
        /// </summary>
        /// <param name="properties"></param>
        [SharePointPermission(SecurityAction.LinkDemand, ObjectModel = true)]
        public override void FeatureInstalled(SPFeatureReceiverProperties properties) {}

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPWebApplication app = properties.Feature.Parent as SPWebApplication;

            IConfigManager configMgr = SharePointServiceLocator.GetCurrent().GetInstance<IConfigManager>();

            DiagnosticsAreaCollection configuredAreas = new DiagnosticsAreaCollection(configMgr);

            try
            {
                foreach (DiagnosticsArea newArea in MyAreas)
                {
                    var existingArea = configuredAreas[newArea.Name];

                    if (existingArea == null)
                    {
                        configuredAreas.Add(newArea);
                    }
                    else
                    {
                        throw new SPException("Diagnostic area already exists");
                    }
                }
                configuredAreas.SaveConfiguration();
                DiagnosticsAreaEventSource.EnsureConfiguredAreasRegistered();
            }
            catch (Exception) {} // TODO - uninstall diagnostic areas

            // Get the ServiceLocatorConfig service from the service locator.
            IServiceLocator serviceLocator = SharePointServiceLocator.GetCurrent();
            IServiceLocatorConfig serviceLocatorConfig =
                serviceLocator.GetInstance<IServiceLocatorConfig>();

            try
            {
                var testInstance = new LoupeLog();
                testInstance.LogInfo("Registered type mapping for Loupe");
                serviceLocatorConfig.RegisterTypeMapping<ILogging, LoupeLog>();
            }
            catch (FileNotFoundException)
            {
                var testInstance = new NaverticaLog();
                testInstance.LogInfo("Registered type mapping for NaverticaLog");
                serviceLocatorConfig.RegisterTypeMapping<ILogging, NaverticaLog>();
            }

            SharePointServiceLocator.Reset();
        }


        // Uncomment the method below to handle the event raised before a feature is deactivated.

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            SPWebApplication app = properties.Feature.Parent as SPWebApplication;
            IServiceLocator serviceLocator = SharePointServiceLocator.GetCurrent();
            IServiceLocatorConfig serviceLocatorConfig =
                serviceLocator.GetInstance<IServiceLocatorConfig>();


            serviceLocatorConfig.RemoveTypeMappings<ILogging>();
            SharePointServiceLocator.Reset();
        }


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