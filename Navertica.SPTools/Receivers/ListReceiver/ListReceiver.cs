using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

namespace Navertica.SharePoint.Receivers
{
    /// <summary>
    /// List Events
    /// </summary>
    public class ListReceiver : SPListEventReceiver
    {
       /// <summary>
       /// A field was added.
       /// </summary>
       public override void FieldAdded(SPListEventProperties properties)
       {
           base.FieldAdded(properties);
       }

       /// <summary>
       /// A field is being added.
       /// </summary>
       public override void FieldAdding(SPListEventProperties properties)
       {
           base.FieldAdding(properties);
       }

       /// <summary>
       /// A field was removed.
       /// </summary>
       public override void FieldDeleted(SPListEventProperties properties)
       {
           base.FieldDeleted(properties);
       }

       /// <summary>
       /// A field is being removed.
       /// </summary>
       public override void FieldDeleting(SPListEventProperties properties)
       {
           base.FieldDeleting(properties);
       }

       /// <summary>
       /// A field was updated.
       /// </summary>
       public override void FieldUpdated(SPListEventProperties properties)
       {
           base.FieldUpdated(properties);
       }

       /// <summary>
       /// A field is being updated.
       /// </summary>
       public override void FieldUpdating(SPListEventProperties properties)
       {
           base.FieldUpdating(properties);
       }

       /// <summary>
       /// A list is being added.
       /// </summary>
       public override void ListAdding(SPListEventProperties properties)
       {
           base.ListAdding(properties);
       }

       /// <summary>
       /// A list is being deleted.
       /// </summary>
       public override void ListDeleting(SPListEventProperties properties)
       {
           base.ListDeleting(properties);
       }

       /// <summary>
       /// A list was added.
       /// </summary>
       public override void ListAdded(SPListEventProperties properties)
       {
           base.ListAdded(properties);
       }

       /// <summary>
       /// A list was deleted.
       /// </summary>
       public override void ListDeleted(SPListEventProperties properties)
       {
           base.ListDeleted(properties);
       }


    }
}
