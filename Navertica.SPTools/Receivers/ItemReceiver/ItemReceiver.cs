using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint;
using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.Extensions;
using Navertica.SharePoint.Interfaces;
using Navertica.SharePoint.Logging;
using Navertica.SharePoint.RepoService;
using Newtonsoft.Json.Linq;

namespace Navertica.SharePoint.Receivers
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class ItemReceiver : SPItemEventReceiver
    {
        internal string Global = "";

        private void RunAllValid(SPItemEventProperties props)
        {
            ILogging log = NaverticaLog.GetInstance();

            string eventName = Global + props.EventType.ToString();

            if (props.ListItem == null) // ItemAdding 
            {
                log.LogInfo("props.ListItem == null in list ", props.List.DefaultViewUrl, eventName);
                return;
            }

            ConfigServiceClient configurations = new ConfigServiceClient(props.Site);
            List<KeyValuePair<string, object>> executionResults = new List<KeyValuePair<string, object>>();

            try
            {
                ConfigGuidList cfgs = configurations.Filter(props.ListItem, eventName);

                if (cfgs.Count == 0) return;

                log.LogInfo(eventName, "item url", props.ListItem.FormUrlDisplay(true));

                PluginHost.Init(props.Site);

                foreach (var cfgguid in cfgs)
                {
                    var c = configurations.GetBranchJson(cfgguid, ""); //get completeBranch//configurations.GetBranchJson(cfgguid, eventName);
                    if (c != null)
                    {
                        JObject jc = JObject.Parse(c);
                        JObject eventBranch = (JObject) jc[eventName];

                        if (eventBranch == null)
                        {
                            log.LogError("Missing event branch in config: " + eventName);
                            return;
                        }

                        foreach (KeyValuePair<string, JToken /* really string */> kvp in eventBranch)
                        {
                            string pluginName = kvp.Value.ToString();

                            IPlugin plugin = ItemReceiverPluginHost.Get(props.Site, pluginName);
                            if (plugin == null)
                            {
                                log.LogError(string.Format("ItemReceiverScriptPlugin named {0} was not found in loaded plugins, skipping execution", pluginName));
                                continue;
                            }
                            try
                            {
                                // plugin config itself should be in another root branch
                                ItemReceiverPluginConfig pluginConfig = ConfigBranch.GetInstance<ItemReceiverPluginConfig>(jc[kvp.Key].ToString());

                                // on by default, as is ShP standard
                                if (!( pluginConfig.EventFiringEnabled ?? true ))
                                {
                                    this.EventFiringEnabled = false;
                                }

                                object result = plugin.Execute(pluginConfig, props);
                                executionResults.Add(new KeyValuePair<string, object>(kvp.Value.ToString(), result));
                            }
                            catch (Exception e)
                            {

                                log.LogError(string.Format("ItemReceiverScriptPlugin named {0} has thrown {1}", pluginName, e, e.InnerException, e.StackTrace, e.Source));
                            }
                        }
                    }
                }

                string logstr =
                    log.LogInfo("ItemReceiverScriptPlugins ran:\n" + executionResults.Select(
                        kvp =>
                            string.Format("Ran ItemReceiverScriptPlugin|config name {0} with result:{1}\n\n", kvp.Key,
                                ( kvp.Value ?? "null" ).ToString())).JoinStrings("\n"));
            }
            catch (Exception e)
            {
                log.LogException(e);
            }
        }

        #region Receiver Events

        /// <summary>
        /// An item is being added.
        /// </summary>
        public override void ItemAdding(SPItemEventProperties properties)
        {
            base.ItemAdding(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// An item is being updated.
        /// </summary>
        public override void ItemUpdating(SPItemEventProperties properties)
        {
            base.ItemUpdating(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// An item is being deleted.
        /// </summary>
        public override void ItemDeleting(SPItemEventProperties properties)
        {
            base.ItemDeleting(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// An item is being checked in.
        /// </summary>
        public override void ItemCheckingIn(SPItemEventProperties properties)
        {
            base.ItemCheckingIn(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// An item is being checked out.
        /// </summary>
        public override void ItemCheckingOut(SPItemEventProperties properties)
        {
            base.ItemCheckingOut(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// An item is being unchecked out.
        /// </summary>
        public override void ItemUncheckingOut(SPItemEventProperties properties)
        {
            base.ItemUncheckingOut(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// An attachment is being added to the item.
        /// </summary>
        public override void ItemAttachmentAdding(SPItemEventProperties properties)
        {
            base.ItemAttachmentAdding(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// An attachment is being removed from the item.
        /// </summary>
        public override void ItemAttachmentDeleting(SPItemEventProperties properties)
        {
            base.ItemAttachmentDeleting(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// A file is being moved.
        /// </summary>
        public override void ItemFileMoving(SPItemEventProperties properties)
        {
            base.ItemFileMoving(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// An item was added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            base.ItemAdded(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// An item was updated.
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            base.ItemUpdated(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// An item was deleted.
        /// </summary>
        public override void ItemDeleted(SPItemEventProperties properties)
        {
            base.ItemDeleted(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// An item was checked in.
        /// </summary>
        public override void ItemCheckedIn(SPItemEventProperties properties)
        {
            base.ItemCheckedIn(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// An item was checked out.
        /// </summary>
        public override void ItemCheckedOut(SPItemEventProperties properties)
        {
            base.ItemCheckedOut(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// An item was unchecked out.
        /// </summary>
        public override void ItemUncheckedOut(SPItemEventProperties properties)
        {
            base.ItemUncheckedOut(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// An attachment was added to the item.
        /// </summary>
        public override void ItemAttachmentAdded(SPItemEventProperties properties)
        {
            base.ItemAttachmentAdded(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// An attachment was removed from the item.
        /// </summary>
        public override void ItemAttachmentDeleted(SPItemEventProperties properties)
        {
            base.ItemAttachmentDeleted(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// A file was moved.
        /// </summary>
        public override void ItemFileMoved(SPItemEventProperties properties)
        {
            base.ItemFileMoved(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// A file was converted.
        /// </summary>
        public override void ItemFileConverted(SPItemEventProperties properties)
        {
            base.ItemFileConverted(properties);
            RunAllValid(properties);
        }

        /// <summary>
        /// The list received a context event.
        /// </summary>
        public override void ContextEvent(SPItemEventProperties properties)
        {
            base.ContextEvent(properties);
            RunAllValid(properties);
        }

        #endregion
    }
}