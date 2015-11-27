using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint;
using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.Extensions;
using Navertica.SharePoint.Interfaces;
using Navertica.SharePoint.Logging;

namespace Navertica.SharePoint.Plugins
{
    public class DuplicateFields : IPlugin
    {
        private ILogging log = NaverticaLog.GetInstance();

        #region Plugin Settings

        public List<PluginHost> PluginScope()
        {
            return new List<PluginHost> { new ItemReceiverPluginHost() };
        }

        public string Name()
        {
            return this.GetType().Name;
        }

        public string Description()
        {
            return "Duplicate receiver";
        }

        public string Version()
        {
            return "1.0";
        }

        public IPlugin GetInstance()
        {
            return new DuplicateFields();
        }

        #endregion

        public IList<KeyValuePair<bool, string>> Test(ConfigBranch config, object context)
        {
            throw new NotImplementedException();
        }

        public class DuplicatePluginConfig : ConfigBranch
        {
            private List<Dictionary<string, List<string>>> _duplicateFields;

            public List<Dictionary<string, List<string>>> DuplicateFields
            {
                get { return _duplicateFields; }
                set { _duplicateFields = value; }
            }

            public List<string> UsedFields
            {
                get
                {
                    List<string> fields = new List<string>();
                    foreach (Dictionary<string, List<string>> dict in DuplicateFields)
                    {
                        List<string> f = dict.Values.Select(v => v.First()).ToList();
                        fields.AddRange(f);
                    }

                    return fields;
                }
            }
        }

        public object Execute(ConfigBranch config, object context)
        {
            if (context == null) throw new ArgumentNullException("context");

            SPListItem item = null; 

            var properties = context as SPItemEventProperties;
            if (properties != null)
            {
                item = properties.ListItem;
            }

            if (context.GetType() == typeof (SPListItem))
            {
                item = context as SPListItem;
            }

            var duplicatePluginConfig = ConfigBranch.GetInstance<DuplicatePluginConfig>(config);
            return DuplicateValues(item, duplicatePluginConfig);
        }

        public string Install(IEnumerable<string> urls, IDictionary<string, object> context)
        {
            string res = "";
            List<SPEventReceiverType> events = new List<SPEventReceiverType>() { SPEventReceiverType.ItemAdded, SPEventReceiverType.ItemUpdated };

            foreach (var url in urls)
            {
                try
                {
                    using (SPWeb web = ( context["site"] as SPSite ).OpenW(url, true))
                    {
                        SPList list = web.OpenList(url, true);
                        foreach (var eve in events)
                        {
                            if (list.AttachEventReceiver("SPTools2013, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4cbd3bf6fd3ee278", "Navertica.SharePoint.Receivers.ItemReceiver", eve))
                            {
                                res += "Adedd " + eve + " at " + list.AbsoluteUrl() + "\n";
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    log.LogException(exc, "Failed to install plugin " + this.Name());
                }
            }

            if (res != "")
            {
                log.LogInfo(res);
            }

            return res;
        }

        private object DuplicateValues(SPListItem item, DuplicatePluginConfig config)
        {
            if (item == null) throw new ArgumentNullException("item");

            log.LogInfo("DUPLICATE on item " + item.FormUrlDisplay());

            if (config.DuplicateFields == null) throw new Exception("Configuration has no values to duplicate");

            //Teoreticky by nemelo nikdy nastat
            if (!item.ParentList.ContainsFieldIntName(config.UsedFields))
            {
                throw new SPFieldNotFoundException(item.ParentList, config.UsedFields);
            }

            foreach (Dictionary<string, List<string>> dict in config.DuplicateFields)
            {
                SPField fromField = item.ParentList.OpenField(dict["FROM-FIELD"].First());
                SPField toField = item.ParentList.OpenField(dict["TO-FIELD"].First());

                Type fromType = fromField.GetType();
                Type toType = toField.GetType();
                bool toTypeIsText = ( toType == typeof (SPFieldText) || toType == typeof (SPFieldMultiLineText) );

                object valueToStore = item[fromField.InternalName];
                if (valueToStore != null)
                {
                    valueToStore = item[fromField.InternalName].ToString();

                    //budou urcite zakazane typy do kterych nelze zapisovat
                    if (toType == typeof (SPFieldCalculated) || toType == typeof (SPFieldCalculated))
                    {
                        log.LogError("Cant copy value from field '" + fromField + "' to field '" + toField + "' of type " + toType);
                        continue;
                    }

                    if (fromType == typeof(SPFieldAttachments)) { }

                    if (fromType == typeof (SPFieldBoolean))
                    {
                        if (toType == typeof (SPFieldNumber)) valueToStore = valueToStore.ToBool() ? 1 : 0;
                    }

                    if (fromType == typeof(SPFieldAllDayEvent)) { }
                    if (fromType == typeof (SPFieldCalculated))
                    {
                        if (toTypeIsText)
                        {
                            valueToStore = ( (string) valueToStore ).GetLookupValues();
                        }
                        if (toType == typeof (SPFieldNumber))
                        {
                            valueToStore = ( (string) valueToStore ).GetLookupIndex();
                        }
                    }
                    if (fromType == typeof(SPFieldChoice)) { }
                    if (fromType == typeof(SPFieldComputed)) { }
                    if (fromType == typeof(SPFieldCrossProjectLink)) { }
                    if (fromType == typeof(SPFieldCurrency)) { }
                    if (fromType == typeof(SPFieldDateTime)) { }
                    if (fromType == typeof(SPFieldDecimal)) { }
                    if (fromType == typeof(SPFieldFile)) { }
                    if (fromType == typeof(SPFieldGeolocation)) { }
                    if (fromType == typeof(SPFieldIndex)) { }
                    if (fromType == typeof(SPFieldGuid)) { }
                    if (fromType == typeof(SPFieldLink)) { }
                    if (fromType == typeof (SPFieldLookup))
                    {
                        if (toTypeIsText)
                        {
                            valueToStore = ( (string) valueToStore ).GetLookupValues();
                        }

                    }
                    if (fromType == typeof (SPFieldMultiChoice))
                    {
                        if (toTypeIsText)
                        {
                            List<string> vals = ( valueToStore ?? "" ).ToString().Split(";#").Where(v => !string.IsNullOrEmpty(v)).ToList();
                            valueToStore = vals.JoinStrings("; ");
                        }
                    }
                    if (fromType == typeof(SPFieldMultiColumn)) { }
                    if (fromType == typeof(SPFieldMultiLineText)) { }
                    if (fromType == typeof(SPFieldNumber)) { }

                    if (fromType == typeof(SPFieldPageSeparator)) { }
                    if (fromType == typeof(SPFieldRecurrence)) { }
                    if (fromType == typeof(SPFieldText)) { }
                    if (fromType == typeof(SPFieldUrl)) { }
                    if (fromType == typeof (SPFieldUser))
                    {
                        if (toTypeIsText)
                        {
                            List<SPPrincipal> principals = item.Web.GetSPPrincipals(item[fromField.InternalName]);
                            valueToStore = principals.GetNames().JoinStrings(", ");
                        }
                    }
                }

                item[toField.InternalName] = valueToStore;
            }

            try
            {
                item.SystemUpdate(false, false); // bez false bude hazet System.Runtime.InteropServices.COMException (0x81020037)
            }
            catch (Exception saveException)
            {
                log.LogException(saveException, "Item update problem");
            }
            return "OK";
        }
    }
}

/*
 {
  "DuplicateFields--I": {
    "DuplicateFields": [
      {
        "FROM-FIELD": [
          "Title"
        ],
        "TO-FIELD": [
          "TitleDup"
        ]
      }
    ]
  },
  "ItemAdded": {
    "DuplicateFields--I": "DuplicateFields"
  },
  "ItemUpdated": {
    "DuplicateFields--I": "DuplicateFields"
  }
}

 */
