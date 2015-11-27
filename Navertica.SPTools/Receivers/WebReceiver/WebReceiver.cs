using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

namespace Navertica.SharePoint.Receivers
{
    /// <summary>
    /// Web Events
    /// </summary>
    public class WebReceiver : SPWebEventReceiver
    {
       /// <summary>
       /// A site collection is being deleted.
       /// </summary>
       public override void SiteDeleting(SPWebEventProperties properties)
       {
           base.SiteDeleting(properties);
       }

       /// <summary>
       /// A site is being deleted.
       /// </summary>
       public override void WebDeleting(SPWebEventProperties properties)
       {
           base.WebDeleting(properties);
       }

       /// <summary>
       /// A site is being moved.
       /// </summary>
       public override void WebMoving(SPWebEventProperties properties)
       {
           base.WebMoving(properties);
       }

       /// <summary>
       /// A site is being provisioned.
       /// </summary>
       public override void WebAdding(SPWebEventProperties properties)
       {
           base.WebAdding(properties);
       }

       /// <summary>
       /// A site collection was deleted.
       /// </summary>
       public override void SiteDeleted(SPWebEventProperties properties)
       {
           base.SiteDeleted(properties);
       }

       /// <summary>
       /// A site was deleted.
       /// </summary>
       public override void WebDeleted(SPWebEventProperties properties)
       {
           base.WebDeleted(properties);
       }

       /// <summary>
       /// A site was moved.
       /// </summary>
       public override void WebMoved(SPWebEventProperties properties)
       {
           base.WebMoved(properties);
       }

       /// <summary>
       /// A site was provisioned.
       /// </summary>
       public override void WebProvisioned(SPWebEventProperties properties)
       {
           base.WebProvisioned(properties);
       }


    }
}
