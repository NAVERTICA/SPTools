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
