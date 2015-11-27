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
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Navertica.SharePoint.RepoService;

namespace Navertica.SharePoint.SPTools
{
    public partial class GetScript : LayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SPSite site = SPContext.Current.Site;

            RepoServiceClient repo = new RepoServiceClient(site);
            Response.ContentType = "text/javascript";

            string[] scriptNames = Request.QueryString["FilePaths"].Split(";");

            string results = "";

            foreach (string scriptName in scriptNames)
            {
                if (( scriptName ?? "" ).Trim() == "") continue;
                results += repo.Get(scriptName) + "\n";
            }

            Response.Write(results);
        }
    }
}