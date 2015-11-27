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

using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.ConfigService.Service;

namespace Navertica.SharePoint.ConfigService.Administration
{
    using System;
    using System.Globalization;
    using System.Web;
    using System.Web.UI.WebControls;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.ApplicationPages;
    using Microsoft.SharePoint.Utilities;
    using Microsoft.SharePoint.WebControls;
    using Administration;

    /// <summary>
    /// The Application Properties Page.
    /// </summary>
    public partial class PropertiesPage : BaseAdminPage
    {
        #region Fields

        #endregion

        #region Properties

        /// <summary>
        /// The required query string parameters
        /// </summary>
        protected override string[] RequiredPageParameters
        {
            get
            {
                return new string[] { "id" };
            }
        }

        #endregion

        #region Page Events

        /// <summary>
        /// Page_Load event.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">The EventArgs.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.ServiceApplication == null)
            {
                SPUtility.TransferToErrorPage(HttpContext.GetGlobalResourceObject("NVRConfigService.ServiceAdminResources", "ErrorNoServiceApplication", CultureInfo.CurrentCulture).ToString());
            }

            ((DialogMaster)Page.Master).OkButton.Click += new EventHandler(this.OkButton_Click);

            if (!this.IsPostBack)
            {
                (this.applicationPoolSection as IisWebServiceApplicationPoolSection).SetSelectedApplicationPool(this.ServiceApplication.ApplicationPool);
                this.textBoxServiceName.Text = this.ServiceApplication.Name;
            }

            // set masterpage's ok button as submit for form
            Form.DefaultButton = ((DialogMaster)Master).OkButton.UniqueID;
        }

        #endregion

        #region Control Events

        /// <summary>
        /// Click event.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">The EventArgs.</param>
        protected void OkButton_Click(object sender, EventArgs e)
        {
            this.Validate();

            if (this.IsValid)
            {
                using (SPLongOperation operation = new SPLongOperation(this))
                {
                    operation.Begin();
                    this.ServiceApplication.Name = this.textBoxServiceName.Text.Trim();
                    IisWebServiceApplicationPoolSection iisSection = this.applicationPoolSection as IisWebServiceApplicationPoolSection;
                    this.ServiceApplication.ApplicationPool = iisSection.GetOrCreateApplicationPool();
                    this.ServiceApplication.Update();
                    operation.EndScript("window.frameElement.commitPopup();");
                }
            }
        }

        /// <summary>
        /// ServerValidate event.
        /// </summary>
        /// <param name="sender">The Sender.</param>
        /// <param name="e">The EventArgs.</param>
        protected void CustomValidatorServiceNameDuplicate_ServerValidate(object sender, ServerValidateEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            string serviceName = e.Value;

            if (string.Equals(this.ServiceApplication.Name, serviceName, StringComparison.CurrentCultureIgnoreCase))
            {
                e.IsValid = true;
                return;
            }

            // Get the service
            NVRConfigService service = NVRConfigService.GetOrCreateService();

            // Try to get a duplicate service application
            ConfigServiceApplication duplicate = SPFarm.Local.GetObject(serviceName, service.Id, typeof(ConfigServiceApplication)) as ConfigServiceApplication;

            if (duplicate != null)
            {
                e.IsValid = false;
            }
            else
            {
                e.IsValid = true;
            }
        }

        #endregion
    }
}
