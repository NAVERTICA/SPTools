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
    /// List Workflow Events
    /// </summary>
    public class WorkflowReceiver : SPWorkflowEventReceiver
    {
       /// <summary>
       /// A workflow is starting.
       /// </summary>
       public override void WorkflowStarting(SPWorkflowEventProperties properties)
       {
           base.WorkflowStarting(properties);
       }

       /// <summary>
       /// A workflow was started.
       /// </summary>
       public override void WorkflowStarted(SPWorkflowEventProperties properties)
       {
           base.WorkflowStarted(properties);
       }

       /// <summary>
       /// A workflow was postponed.
       /// </summary>
       public override void WorkflowPostponed(SPWorkflowEventProperties properties)
       {
           base.WorkflowPostponed(properties);
       }

       /// <summary>
       /// A workflow was completed.
       /// </summary>
       public override void WorkflowCompleted(SPWorkflowEventProperties properties)
       {
           base.WorkflowCompleted(properties);
       }


    }
}
