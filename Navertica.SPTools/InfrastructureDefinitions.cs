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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Navertica.SharePoint.Interfaces;
using Navertica.SharePoint.Logging;
using SPMeta2.Definitions;
using SPMeta2.Enumerations;
using SPMeta2.Extensions;
using SPMeta2.Models;
using SPMeta2.SSOM.Services;
using SPMeta2.Syntax.Default;

namespace Navertica.SharePoint.Infrastructure
{
    /// <summary>
    ///  Definitions for all our required structures - a SiteConfig list and SiteScripts library
    /// </summary>
    public static class SiteStructuresDefinitions
    {
        #region SiteConfig

        public static List<string> SiteConfigFields
        {
            get
            {
                return new List<string>
                {
                    SiteConfigApp.InternalName,
                    SiteConfigPackage.InternalName,
                    SiteConfigUrl.InternalName,
                    SiteConfigContentType.InternalName,
                    SiteConfigListType.InternalName,
                    SiteConfigOrder.InternalName,
                    SiteConfigJSON.InternalName,
                    SiteConfigActive.InternalName,
                    SiteConfigApproved.InternalName,
                    SiteConfigActiveFor.InternalName,
                    SiteConfigDebug.InternalName,
                    //"test", "BLALBAL"
                };
            }
        }

        #region Fields

        private static FieldDefinition SiteConfigApp = new FieldDefinition
        {
            Id = new Guid("033C7704-5F74-4959-808A-4B73B50987B0"),
            Title = "Apps",
            Description = "Apps present in this config",
            InternalName = "NVR_SiteConfigApp",
            FieldType = BuiltInFieldTypes.Note,
            Group = Const.DEFAULT_GROUP,
            Required = true,
        };

        private static FieldDefinition SiteConfigPackage = new FieldDefinition
        {
            Id = new Guid("ADE40A13-0D3F-48E5-8C94-4E44C5EB19B1"),
            Title = "Package",
            Description = "Configs get grouped by this value, and together with Title it forms a unique key",
            InternalName = "NVR_SiteConfigPackage",
            FieldType = BuiltInFieldTypes.Text,
            Group = Const.DEFAULT_GROUP,
        };

        private static FieldDefinition SiteConfigUrl = new FieldDefinition
        {
            Id = new Guid("758AA270-379B-481E-8402-FB21F64D522C"),
            Title = "Url",
            Description = "Config will be active only for these relative URLs (separated by pipe |, with * and ? as wildcards), i.e.: '/web1/MyList/*Form.aspx' will be active for all forms of MyList in web1",
            InternalName = "NVR_SiteConfigUrl",
            FieldType = BuiltInFieldTypes.Note,
            Group = Const.DEFAULT_GROUP,
            JSLink = "~/_layouts/15/NVR.SPTools/UrlJSLink.js"
        };

        private static FieldDefinition SiteConfigContentType = new FieldDefinition
        {
            Id = new Guid("C85A4EAC-5CC0-4DF4-A258-705B26B5F716"),
            Title = "CTs",
            Description = "Config will be active only for these content type ids (with * as wildcard, empty = all content types)",
            InternalName = "NVR_SiteConfigContentType",
            FieldType = BuiltInFieldTypes.Note,
            Group = Const.DEFAULT_GROUP,
        };

        private static FieldDefinition SiteConfigListType = new FieldDefinition
        {
            Id = new Guid("7B1AF81E-36B1-4B19-B992-0ECEE7E0B35B"),
            Title = "List Type",
            Description = "Config will be active only for these list types (with * as wildcard, empty = all content types)",
            InternalName = "NVR_SiteConfigListType",
            FieldType = BuiltInFieldTypes.Note,
            Group = Const.DEFAULT_GROUP,
        };

        private static FieldDefinition SiteConfigOrder = new FieldDefinition
        {
            Id = new Guid("80AA5463-4EC7-41B4-86F4-AB6327988A79"),
            Title = "Order",
            Description = "Order of execution in case several configs with a common app are valid",
            InternalName = "NVR_SiteConfigOrder",
            FieldType = BuiltInFieldTypes.Number,
            Group = Const.DEFAULT_GROUP,
        };

        private static FieldDefinition SiteConfigJSON = new FieldDefinition
        {
            Id = new Guid("96AE53A8-381E-47D4-9651-F43AAA9F26D5"),
            Title = "Config",
            Description = "JSON config editor",
            InternalName = "NVR_SiteConfigJSON",
            FieldType = "JSONField",
            Group = Const.DEFAULT_GROUP,
        };

        private static FieldDefinition SiteConfigActive = new FieldDefinition
        {
            Id = new Guid("37AB5B04-EA3A-49B8-89AA-60F4A7518AED"),
            Title = "Active",
            Description = "Config can be disabled with this switch",
            InternalName = "NVR_SiteConfigActive",
            FieldType = BuiltInFieldTypes.Boolean,
            Group = Const.DEFAULT_GROUP,
        };

        private static FieldDefinition SiteConfigApproved = new FieldDefinition
        {
            Id = new Guid("193A52C2-DF47-4E05-ACB8-A03ED73455E6"),
            Title = "Approved",
            Description = "Config is only active for everyone if Approved",
            InternalName = "NVR_SiteConfigApproved",
            FieldType = BuiltInFieldTypes.Boolean,
            Group = Const.DEFAULT_GROUP,
        };

        private static FieldDefinition SiteConfigActiveFor = new FieldDefinition
        {
            Id = new Guid("625A9492-3CED-4DA6-AFE2-B04B7077686B"),
            Title = "Active for",
            Description = "Unless Approved is checked, Config will only be active for users/groups in this field",
            InternalName = "NVR_SiteConfigActiveFor",
            FieldType = BuiltInFieldTypes.UserMulti,
            Group = Const.DEFAULT_GROUP,
        };

        private static FieldDefinition SiteConfigDebug = new FieldDefinition
        {
            Id = new Guid("0308B14F-E764-46B3-8436-4A1565E5361C"),
            Title = "Debug",
            Description = "Debug can be disabled with this switch",
            InternalName = "NVR_SiteConfigDebug",
            FieldType = BuiltInFieldTypes.Boolean,
            Group = Const.DEFAULT_GROUP,
        };

        #endregion

        private static ContentTypeDefinition SiteConfigJSON_CT = new ContentTypeDefinition
        {
            ParentContentTypeId = "0x01",
            Group = Const.DEFAULT_GROUP,
            Id = new Guid("00AF00A1-9FBA-41D6-8E7F-DB37BCB1847B"),
            Name = "NVR_SiteConfigJSON",
        };

        private static ListDefinition SiteConfigList = new ListDefinition
        {
            Title = "SiteConfig",
            CustomUrl = "Lists/SiteConfig",
            ContentTypesEnabled = true,
            TemplateType = BuiltInListTemplateTypeId.GenericList
        };

        private static ListViewDefinition SiteConfigView = new ListViewDefinition
        {
            Title = "JSON",
            IsDefault = true,
            RowLimit = 100,
            Query = @"
<Where>
    <Eq>
        <FieldRef Name='ContentType' />
        <Value Type='Computed'>NVR_SiteConfigJSON</Value>
    </Eq>
</Where>
<GroupBy Collapse=""FALSE"" GroupLimit=""100"">
    <FieldRef Name=""NVR_SiteConfigPackage"" Ascending=""FALSE"" />
</GroupBy>
<OrderBy>
    <FieldRef Name=""NVR_SiteConfigPackage"" Ascending=""TRUE"" />
    <FieldRef Name=""NVR_SiteConfigOrder"" Ascending=""TRUE"" />
</OrderBy>",
            Fields = new System.Collections.ObjectModel.Collection<string>
            {
                BuiltInInternalFieldNames.Edit,
                BuiltInInternalFieldNames.LinkTitle,
                "NVR_SiteConfigPackage",
                "NVR_SiteConfigOrder",
                "NVR_SiteConfigActive",
                "NVR_SiteConfigApproved",
                "NVR_SiteConfigUrl",
                "NVR_SiteConfigContentType",
                "NVR_SiteConfigListType",
                BuiltInInternalFieldNames.Created,
                BuiltInInternalFieldNames.Author,
                BuiltInInternalFieldNames.Modified,
                BuiltInInternalFieldNames.Editor,
            }
        };

        #endregion

        #region SiteScripts

        private static ContentTypeDefinition SiteScripts_CT = new ContentTypeDefinition
        {
            ParentContentTypeId = "0x0101",
            Group = Const.DEFAULT_GROUP,
            Id = new Guid("6792B8BF-84E7-480A-B949-9E469830FE6A"),
            Name = "NVR_SiteScripts",
        };

        private static ListDefinition SiteScriptsLibrary = new ListDefinition
        {
            Title = "SiteScripts",
            CustomUrl = "SiteScripts",
            ContentTypesEnabled = true,
            TemplateType = BuiltInListTemplateTypeId.DocumentLibrary
        };

        private static ListViewDefinition SiteScriptsView = new ListViewDefinition
        {
            Title = "Scripts2013",
            IsDefault = false,
            RowLimit = 100,
            Query = string.Format("<OrderBy><FieldRef Name='{0}' Ascending='FALSE' /></OrderBy>", BuiltInInternalFieldNames.Modified),
            Fields = new System.Collections.ObjectModel.Collection<string>
            {
                BuiltInInternalFieldNames.Edit,
                BuiltInInternalFieldNames.LinkFilename,
                BuiltInInternalFieldNames.File_x0020_Type,
                BuiltInInternalFieldNames.Created,
                BuiltInInternalFieldNames.Author,
                BuiltInInternalFieldNames.Modified,
                BuiltInInternalFieldNames.Editor,
            }
        };

        #endregion

        #region Receiver Definitions

        private static EventReceiverDefinition ItemEventReceiverAdding = new EventReceiverDefinition
        {
            Assembly = Const.NVR_DLL,
            Class = "Navertica.SharePoint.Receivers.ItemReceiver",
            SequenceNumber = 10,
            Synchronization = BuiltInEventReceiverSynchronization.Default,
            Type = BuiltInEventReceiverType.ItemAdding,
            Data = "",
            Name = "ItemEventReceiverAdding"
        };

        private static EventReceiverDefinition ItemEventReceiverAdded = new EventReceiverDefinition
        {
            Assembly = Const.NVR_DLL,
            Class = "Navertica.SharePoint.Receivers.ItemReceiver",
            SequenceNumber = 10,
            Synchronization = BuiltInEventReceiverSynchronization.Default,
            Type = BuiltInEventReceiverType.ItemAdded,
            Data = "",
            Name = "ItemEventReceiverAdded"
        };

        private static EventReceiverDefinition ItemEventReceiverUpdating = new EventReceiverDefinition
        {
            Assembly = Const.NVR_DLL,
            Class = "Navertica.SharePoint.Receivers.ItemReceiver",
            SequenceNumber = 10,
            Synchronization = BuiltInEventReceiverSynchronization.Default,
            Type = BuiltInEventReceiverType.ItemUpdating,
            Data = "",
            Name = "ItemEventReceiverUpdating"
        };

        private static EventReceiverDefinition ItemEventReceiverUpdated = new EventReceiverDefinition
        {
            Assembly = Const.NVR_DLL,
            Class = "Navertica.SharePoint.Receivers.ItemReceiver",
            SequenceNumber = 10,
            Synchronization = BuiltInEventReceiverSynchronization.Default,
            Type = BuiltInEventReceiverType.ItemUpdated,
            Data = "",
            Name = "ItemEventReceiverUpdated"
        };

        #endregion

        public static void DeploySiteModel(SPSite s)
        {
            var model = SPMeta2Model
                .NewSiteModel(site =>
                {
                    site
                        .AddField(SiteConfigApp)
                        .AddField(SiteConfigPackage)
                        .AddField(SiteConfigUrl)
                        .AddField(SiteConfigContentType)
                        .AddField(SiteConfigListType)
                        .AddField(SiteConfigOrder)
                        .AddField(SiteConfigJSON)
                        .AddField(SiteConfigActive)
                        .AddField(SiteConfigApproved)
                        .AddField(SiteConfigActiveFor)
                        .AddField(SiteConfigDebug)
                        .AddContentType(SiteConfigJSON_CT, jsonCT => jsonCT
                            .AddContentTypeFieldLink(SiteConfigApp)
                            .AddContentTypeFieldLink(SiteConfigPackage)
                            .AddContentTypeFieldLink(SiteConfigUrl)
                            .AddContentTypeFieldLink(SiteConfigContentType)
                            .AddContentTypeFieldLink(SiteConfigListType)
                            .AddContentTypeFieldLink(SiteConfigOrder)
                            .AddContentTypeFieldLink(SiteConfigJSON)
                            .AddContentTypeFieldLink(SiteConfigActive)
                            .AddContentTypeFieldLink(SiteConfigApproved)
                            .AddContentTypeFieldLink(SiteConfigActiveFor)
                            .AddContentTypeFieldLink(SiteConfigDebug))
                        .AddContentType(SiteScripts_CT);
                });

            var ssomProvisionService = new SSOMProvisionService();
            ssomProvisionService.DeployModel(SPMeta2.SSOM.ModelHosts.SiteModelHost.FromSite(s), model);
        }

        public static void DeployRootWebStructures(SPSite site)
        {
            ILogging log = NaverticaLog.GetInstance();

            ModelNode model;

            var rootWeb = new RootWebDefinition();

            try
            {
                model = SPMeta2Model.NewSiteModel(saitou => saitou.AddRootWeb(rootWeb, web => web
                    .AddList(SiteConfigList, list => list
                        .AddContentTypeLink(SiteConfigJSON_CT)
                        .AddListView(SiteConfigView)
                        .AddEventReceiver(ItemEventReceiverAdded)
                        .AddEventReceiver(ItemEventReceiverUpdated))
                    .AddList(SiteScriptsLibrary, lib => lib
                        .AddContentTypeLink(SiteScripts_CT)
                        .AddListView(SiteScriptsView)
                        .AddEventReceiver(ItemEventReceiverAdded)
                        .AddEventReceiver(ItemEventReceiverUpdated)
                    )));
            }
            catch (Exception exc)
            {
                log.LogException("Build SiteStructure model ERROR", exc);
                throw;
            }

            using (SPWeb web = site.OpenWeb("/"))
            {
                try
                {
                    web.AllowUnsafeUpdates = true;
                    var ssomProvisionService = new SSOMProvisionService();

                    ssomProvisionService.DeployModel(SPMeta2.SSOM.ModelHosts.SiteModelHost.FromSite(site), model);
                    web.AllowUnsafeUpdates = false;
                }
                catch (Exception exc)
                {
                    log.LogException("Provision SiteStructure model ERROR", exc);
                    throw;
                }
            }
        }
    }
}