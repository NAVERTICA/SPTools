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
