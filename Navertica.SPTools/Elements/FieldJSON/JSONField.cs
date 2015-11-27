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