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
using Microsoft.SharePoint;
using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.Extensions;
using Navertica.SharePoint.Interfaces;
using Navertica.SharePoint.Logging;

namespace Navertica.SharePoint.Plugins
{
    public class SetRights : IPlugin
    {
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
            return "SetRights receiver";
        }

        public string Version()
        {
            return "1.0";
        }

        public IPlugin GetInstance()
        {
            return new SetRights();
        }

        #endregion

        public IList<KeyValuePair<bool, string>> Test(ConfigBranch config, object context)
        {
            throw new NotImplementedException();
        }

        private readonly ILogging log = NaverticaLog.GetInstance();

        private string setRightsLog = string.Empty;

        #region CONFIG Clases

        public class SetRightsPluginConfig : ItemReceiverPluginConfig
        {
            private Dictionary<string, RightsSettings> _rights;

            public Dictionary<string, RightsSettings> Rights
            {
                get { return _rights; }
                set { _rights = value; }
            }
        }

        public class RightsSettings
        {
            private string _rightsFor;

            public string RightsFor
            {
                get { return _rightsFor; }
                set { _rightsFor = value; }
            }

            private bool _clear;

            public bool Clear
            {
                get { return _clear; }
                set { _clear = value; }
            }

            private bool _expandUsers;

            public bool ExpandUsers
            {
                get { return _expandUsers; }
                set { _expandUsers = value; }
            }

            private List<RoleItem> _roles;

            public List<RoleItem> Roles
            {
                get { return _roles; }
                set { _roles = value; }
            }
        }

        public class RoleItem
        {
            private int _role_Name;

            public int Role_Name
            {
                get { return _role_Name; }
                set { _role_Name = value; }
            }

            private List<string> _for;

            public List<string> For
            {
                get { return _for; }
                set { _for = value; }
            }
        }

        #endregion

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

            var setRightsPluginConfig = ConfigBranch.GetInstance<SetRightsPluginConfig>(config);

            return Execute(item, setRightsPluginConfig);
        }

        private object Execute(SPListItem i, SetRightsPluginConfig config)
        {
            if (i == null) throw new ArgumentNullException("i");

            //polozku otevreme jako elevated a zni jiz pak vse bude elevated!!!
            i.RunElevated(delegate(SPListItem item)
            {
                log.LogInfo("Start ExecuteSetRights from item " + item.FormUrlDisplay());

                foreach (KeyValuePair<string, RightsSettings> right in config.Rights)
                {
                    string rightsTitle = right.Key;
                    RightsSettings settings = right.Value;

                    string itemIdentification = settings.RightsFor;

                    //Prava primo pro polozku
                    if (itemIdentification == "self")
                    {
                        SetRightsForItem(item, settings.Roles, settings.Clear, settings.ExpandUsers);
                    }
                    else if (item.ParentList.ContainsFieldIntName(itemIdentification)) //Prava na lookupovanou polozku
                    {
                        try
                        {
                            SPField field = item.ParentList.OpenField(itemIdentification, true);
                            if (field.IsLookup())
                            {
                                item.GetItemsFromLookup(field.InternalName).ProcessItems(item.Web.Site, delegate(SPListItem lookupItem)
                                {
                                    SetRightsForItem(lookupItem, settings.Roles, settings.Clear, settings.ExpandUsers);
                                    return null;
                                });
                            }
                            else
                            {
                                log.LogError("Field '" + field.InternalName + "' is not lookup");
                                continue;
                            }
                        }
                        catch (Exception exc)
                        {
                            log.LogError(exc);
                        }
                    }
                    else if (itemIdentification.Contains('|')) ////Prava na lookupovanou polozku a lookup v ni
                    {
                        WebListItemDictionary wild = item.TraverseItemLookups(itemIdentification.Split('|').ToList());

                        wild.ProcessItems(item.Web.Site, delegate(SPListItem lookupItem)
                        {
                            Console.WriteLine(lookupItem.Title);
                            SetRightsForItem(lookupItem, settings.Roles, settings.Clear, settings.ExpandUsers);

                            return null;
                        });
                    }
                    else
                    {
                        log.LogError("Could not recognize rights for: '" + itemIdentification + "'");
                    }
                }

                return null;
            });

            log.LogInfo(setRightsLog);

            return "OK";
        }

        private void SetRightsForItem(SPListItem item, List<RoleItem> roles, bool clear, bool expandUsers)
        {

            setRightsLog += item.FormUrlDisplay() + " | ClearRights:" + clear + " | ExpandUsers:" + expandUsers + "\n";

            if (clear) item.RemoveRights();

            foreach (RoleItem role in roles)
            {
                List<SPPrincipal> users = LoadUsers(item, role.For, expandUsers);
                SPRoleDefinition spRole = item.Web.RoleDefinitions.Cast<SPRoleDefinition>().SingleOrDefault(r => r.Id == role.Role_Name);
                if (spRole == null)
                {
                    log.LogError("Could not find permisson level: '" + role + "'");
                    continue;
                }

                item.SetRights(users, spRole, false);

                setRightsLog += spRole.Name + " : " + users.Select(l => l.LoginNameNormalized()).JoinStrings(", ") + "\n";
            }
        }

        /// <summary>
        /// Nacte uzivatele pro ktere se maji priradit prava
        /// </summary>
        /// <param name="item"></param>
        /// <param name="identifications">muze byt login, jmeno pole ze ktereho se nactou uzivatele a nebo lookup a pak pole ze ktereho se to nacte</param>
        /// <param name="expandUsers"></param>
        /// <returns></returns>
        private static List<SPPrincipal> LoadUsers(SPListItem item, List<string> identifications, bool expandUsers)
        {
            List<SPPrincipal> users = new List<SPPrincipal>();

            foreach (string identification in identifications)
            {
                if (item.ParentList.ContainsFieldIntName(identification)) //je jmeno pole
                {
                    if (item[identification] != null)
                    {
                        var fieldUsers = item.Web.GetSPPrincipals(item[identification]);
                        users.AddRange(fieldUsers);
                    }
                }
                else if (identification.Contains("\\")) //obsahuje login - at uz s claims nebo bez
                {
                    List<SPPrincipal> principals = item.Web.GetSPPrincipals(identification.Split(';')); //bacha aby tady nebylo v claims vice stredniku!! 
                    users.AddRange(principals);
                }
                else if (identification.Contains('|')) //Lookupový zápis
                {
                    List<string> fields = identification.Split('|').ToList();
                    string userField = fields.Last();
                    fields.RemoveAt(fields.Count - 1);

                    WebListItemDictionary wild = item.TraverseItemLookups(fields);

                    wild.ProcessItems(item.Web.Site, delegate(SPListItem lookupItem)
                    {
                        if (lookupItem[userField] != null)
                        {
                            List<SPPrincipal> principals = lookupItem.Web.GetSPPrincipals(lookupItem[userField]);
                            if (principals != null)
                            {
                                users.AddRange(principals);
                            }
                        }
                        return null;
                    });
                }
                else
                {
                    //nemelo by diky konfiguraci nikdy nastat
                }
            }

            users = users.Distinct(new SPPrincipalComparer()).ToList();

            return expandUsers ? item.Web.GetSPUsers(users).Cast<SPPrincipal>().ToList() : users;
        }
    }
}

//KONFIGURATION
/*
{
  "SetRights--I": {
    "Debug": true,
    "EventFiringEnabled": true,
    "ItemUpdated": false,
    "Rights": {
      "test": {
        "RightsFor": "self",
        "Clear": true,
        "ExpandUsers": true,
        "Roles": [
          {
            "Role_Name": "1073741829",
            "For": [
              "i:0#.w|nvrpoint\\jfiala;i:0#.w|nvrpoint\\mhlavac",
              "Author"
            ]
          },
          {
            "Role_Name": "1073741826",
            "For": [
              "i:0#.w|nvrpoint\\mhlavac",
              "Lookup|NVR_Department|NVR_Group"
            ]
          }
        ]
      },
      "DALSI TEST": {
        "RightsFor": "Lookup|NVR_Department|NVR_Group",
        "Clear": true,
        "ExpandUsers": true,
        "Roles": [
          {
            "Role_Name": "1073741825",
            "For": [
              "i:0#.w|nvrpoint\\jmatysek",
              "i:0#.w|nvrpoint\\tpilich"
            ]
          }
        ]
      }
    }
  },
  "ItemUpdated": {
    "SetRights--I": "SetRights"
  },
  "ItemAdded": {
    "SetRights--I": "SetRights"
  }
}
 */
