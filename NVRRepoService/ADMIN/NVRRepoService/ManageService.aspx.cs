//-----------------------------------------------------------------------
// <copyright file="ManageService.aspx.cs" company="">
// Copyright © 
// </copyright>
//-----------------------------------------------------------------------

namespace Navertica.SharePoint.RepoService.Service.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.WebControls;
    using Service;

    /// <summary>
    /// This page is responsible for provisioning (installing) the Service, ServiceProxy, and ServiceInstances.
    /// </summary>
    public partial class ManageServicePage : BaseAdminPage
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Page Events

        /// <summary>
        /// Page_Load event.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">The EventArgs.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                // Populate the controls with default values
                RepoService service = SPFarm.Local.Services.GetValue<RepoService>();
                RepoServiceProxy proxy = SPFarm.Local.ServiceProxies.GetValue<RepoServiceProxy>();

                if (service != null)
                {
                    this.literalServiceStatus.Text = HttpContext.GetGlobalResourceObject("NVRRepoService.ServiceAdminResources", "ManageServiceStatusLabelInstalled", CultureInfo.CurrentCulture).ToString();
                    this.imageServiceStatus.ImageUrl = "/_admin/NVRRepoService/ServiceInstalled.gif";
                    this.buttonInstallService.Visible = false;
                    this.buttonRemoveService.Visible = true;

                    this.serviceInstanceStatusSection.Visible = true;
                    this.serviceApplicationStatusSection.Visible = true;
                }
                else
                {
                    this.literalServiceStatus.Text = HttpContext.GetGlobalResourceObject("NVRRepoService.ServiceAdminResources", "ManageServiceStatusLabelNotInstalled", CultureInfo.CurrentCulture).ToString();
                    this.imageServiceStatus.ImageUrl = "/_admin/NVRRepoService/ServiceNotInstalled.gif";
                    this.buttonInstallService.Visible = true;
                    this.buttonRemoveService.Visible = false;
                    this.buttonInstallServiceInstances.Visible = false;

                    this.serviceInstanceStatusSection.Visible = false;
                    this.serviceApplicationStatusSection.Visible = false;
                }

                if (proxy != null)
                {
                    this.imageServiceProxyStatus.ImageUrl = "/_admin/NVRRepoService/ServiceInstalled.gif";
                    this.literalServiceProxyStatus.Text = HttpContext.GetGlobalResourceObject("NVRRepoService.ServiceAdminResources", "ManageServiceStatusLabelInstalled", CultureInfo.CurrentCulture).ToString();
                }
                else
                {
                    this.imageServiceProxyStatus.ImageUrl = "/_admin/NVRRepoService/ServiceNotInstalled.gif";
                    this.literalServiceProxyStatus.Text = HttpContext.GetGlobalResourceObject("NVRRepoService.ServiceAdminResources", "ManageServiceStatusLabelNotInstalled", CultureInfo.CurrentCulture).ToString();
                }

                // Check if the service instances have been provisioned, and show their status
                List<ServiceInstanceStatus> serviceInstances = new List<ServiceInstanceStatus>();

                foreach (SPServer server in SPFarm.Local.Servers)
                {
                    if (server.Role == SPServerRole.Application || server.Role == SPServerRole.SingleServer || server.Role == SPServerRole.WebFrontEnd)
                    {
                        ServiceInstanceStatus serviceInstance = new ServiceInstanceStatus();
                        serviceInstance.ServerName = server.Name;
                        serviceInstance.ServerId = HttpUtility.UrlEncode(server.Id.ToString());
                        serviceInstances.Add(serviceInstance);

                        RepoServiceInstance instance = server.ServiceInstances.GetValue<RepoServiceInstance>();
                        if (instance == null)
                        {
                            serviceInstance.ServerStatus = HttpContext.GetGlobalResourceObject("NVRRepoService.ServiceAdminResources", "ManageServiceInstanceStatusNotInstalled", CultureInfo.CurrentCulture).ToString();
                            serviceInstance.IsInstalled = false;
                            serviceInstance.ServerStatusImage = "/_admin/NVRRepoService/HLTHFAIL.PNG";
                        }
                        else if (instance.Status == SPObjectStatus.Online)
                        {
                            serviceInstance.ServerStatus = HttpContext.GetGlobalResourceObject("NVRRepoService.ServiceAdminResources", "ManageServiceInstanceStatusStarted", CultureInfo.CurrentCulture).ToString();
                            serviceInstance.IsInstalled = true;
                            serviceInstance.ServerStatusImage = "/_admin/NVRRepoService/HLTHSUCC.PNG";
                        }
                        else
                        {
                            serviceInstance.ServerStatus = HttpContext.GetGlobalResourceObject("NVRRepoService.ServiceAdminResources", "ManageServiceInstanceStatusStopped", CultureInfo.CurrentCulture).ToString();
                            serviceInstance.IsInstalled = true;
                            serviceInstance.ServerStatusImage = "/_admin/NVRRepoService/HLTHERR.PNG";
                        }
                    }
                }

                this.gridViewInstanceStatus.DataSource = serviceInstances;
                this.gridViewInstanceStatus.DataBind();

                if (service != null)
                {
                    this.gridViewApplicationStatus.DataSource = service.Applications;
                    this.gridViewApplicationStatus.DataBind();
                }
            }
        }

        #endregion

        #region Control Events

        /// <summary>
        /// Click event.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">The EventArgs.</param>
        protected void ButtonInstallService_Click(object sender, EventArgs e)
        {
            using (SPLongOperation operation = new SPLongOperation(this))
            {
                operation.LeadingHTML = HttpContext.GetGlobalResourceObject("NVRRepoService.ServiceAdminResources", "ManageServiceInstallOperationLeadingHtml", CultureInfo.CurrentCulture).ToString();
                operation.TrailingHTML = HttpContext.GetGlobalResourceObject("NVRRepoService.ServiceAdminResources", "ManageServiceInstallOperationTrailingHtml", CultureInfo.CurrentCulture).ToString();
                operation.Begin();

                // Install the service
                RepoService service = RepoService.GetOrCreateService();

                // Install the service instances but do not start (provision) them (let the admin do this on the services on server screen).
                RepoServiceInstance.CreateServiceInstances(service);

                // Install the service proxy
                RepoServiceProxy.GetOrCreateServiceProxy();

                operation.End("/_admin/NVRRepoService/ManageService.aspx");
            }
        }

        /// <summary>
        /// Click event.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">The EventArgs.</param>
        protected void ButtonRemoveService_Click(object sender, EventArgs e)
        {
            using (SPLongOperation operation = new SPLongOperation(this))
            {
                operation.LeadingHTML = HttpContext.GetGlobalResourceObject("NVRRepoService.ServiceAdminResources", "ManageServiceRemoveOperationLeadingHtml", CultureInfo.CurrentCulture).ToString();
                operation.TrailingHTML = HttpContext.GetGlobalResourceObject("NVRRepoService.ServiceAdminResources", "ManageServiceRemoveOperationTrailingHtml", CultureInfo.CurrentCulture).ToString();
                operation.Begin();

                RepoService.RemoveService();

                operation.End("/_admin/NVRRepoService/ManageService.aspx");
            }
        }

        /// <summary>
        /// Click event.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">The EventArgs.</param>
        protected void ButtonInstallServiceInstances_Click(object sender, EventArgs e)
        {
            using (SPLongOperation operation = new SPLongOperation(this))
            {
                operation.LeadingHTML = HttpContext.GetGlobalResourceObject("NVRRepoService.ServiceAdminResources", "ManageServiceInstallServiceInstancesOperationLeadingHtml", CultureInfo.CurrentCulture).ToString();
                operation.TrailingHTML = HttpContext.GetGlobalResourceObject("NVRRepoService.ServiceAdminResources", "ManageServiceInstallServiceInstancesOperationTrailingHtml", CultureInfo.CurrentCulture).ToString();
                operation.Begin();

                // Get the service
                RepoService service = RepoService.GetOrCreateService();

                // Create the service instances
                RepoServiceInstance.CreateServiceInstances(service);

                operation.End("/_admin/NVRRepoService/ManageService.aspx");
            }
        }

        #endregion

        #region Internal Classes

        /// <summary>
        /// Class used for data binding to the Service Instance grid view
        /// </summary>
        protected class ServiceInstanceStatus
        {
            /// <summary>
            /// Gets or sets the name of a server in the farm.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by ASP.NET databinding.")]
            public string ServerName { get; set; }

            /// <summary>
            /// Gets or sets the status of the service instance on that server in the farm.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by ASP.NET databinding.")]
            public string ServerStatus { get; set; }

            /// <summary>
            /// Gets or sets the Id of the Server
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by ASP.NET databinding.")]
            public string ServerId { get; set; }

            /// <summary>
            /// Gets or sets a url to an image representing the status of the service instance on the server in the farm.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by ASP.NET databinding.")]
            public string ServerStatusImage { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the service instance is provisioned (installed) on a server in the farm.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by ASP.NET databinding.")]
            public bool IsInstalled { get; set; }
        }

        #endregion
    }
}
