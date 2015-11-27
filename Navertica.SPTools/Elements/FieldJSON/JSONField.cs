using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Navertica.SharePoint.RepoService.Service;

namespace Navertica.SharePoint.Fields
{
    public class JSONField : SPFieldMultiLineText
    {

        private const string JSLinkUrl = "~/_layouts/15/NVR.SPTools/JSONField.js";
        public JSONField(SPFieldCollection fields, string fieldName)
            : base(fields, fieldName)
        {
        }

        public JSONField(SPFieldCollection fields, string typeName, string displayName)
            : base(fields, typeName, displayName)
        {
        }
        
        /// <summary>
        /// Override the JSLink property to return the 
        /// value of our custom JavaScript file.
        /// </summary>
        public override string JSLink
        {
            get
            {
                if (string.IsNullOrEmpty(base.JSLink) || base.JSLink == JSLinkUrl || base.JSLink == "clienttemplates.js") return JSLinkUrl;
                else return base.JSLink;
            }
            set
            {
                base.JSLink = value;
            }
        }
   
    }
}