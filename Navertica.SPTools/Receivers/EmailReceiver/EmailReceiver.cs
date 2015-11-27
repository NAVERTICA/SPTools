using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

namespace Navertica.SharePoint.Receivers
{
    /// <summary>
    /// List Email Events
    /// </summary>
    public class EmailReceiver : SPEmailEventReceiver
    {
       /// <summary>
       /// The list received an e-mail message.
       /// </summary>
       public override void EmailReceived(SPList list, SPEmailMessage emailMessage, String receiverData)
       {
           base.EmailReceived(list, emailMessage, receiverData);           
       }


    }
}
