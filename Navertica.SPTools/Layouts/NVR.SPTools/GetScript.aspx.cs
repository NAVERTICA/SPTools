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