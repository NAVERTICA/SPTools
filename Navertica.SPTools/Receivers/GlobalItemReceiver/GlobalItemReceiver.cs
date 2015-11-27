using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

namespace Navertica.SharePoint.Receivers
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class GlobalItemReceiver : ItemReceiver 
    {
        public GlobalItemReceiver()
        {
            base.Global = "Global";
        }
    }
}