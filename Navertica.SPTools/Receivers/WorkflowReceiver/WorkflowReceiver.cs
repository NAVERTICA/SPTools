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
