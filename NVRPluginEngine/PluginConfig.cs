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
using Navertica.SharePoint.ConfigService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Navertica.SharePoint
{
    /// <summary>
    /// Basic config type for plugin-running functionalities (receivers...)
    /// </summary>
    public class PluginConfig : ConfigBranch
    {
        public List<KeyValuePair<string, string>> ScriptLinksWithConfigNames;

        [NonSerialized] public List<KeyValuePair<string, KeyValuePair<string, JObject>>> PluginConfigs = new List<KeyValuePair<string, KeyValuePair<string, JObject>>>();

        public override string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override object Initialize()
        {
            if (!string.IsNullOrEmpty(( Json ?? "" ).Trim()))
            {
                try
                {
                    dynamic data = JsonConvert.DeserializeObject(Json);

                    ScriptLinksWithConfigNames = new List<KeyValuePair<string, string>>();

                    foreach (var obj in data.ScriptLinksWithConfigNames)
                    {
                        foreach (var prop in obj.Properties())
                        {
                            ScriptLinksWithConfigNames.Add(new KeyValuePair<string, string>(prop.Name, prop.Value.ToString()));
                            break;
                        }
                    }

                    foreach (KeyValuePair<string, string> kvp in ScriptLinksWithConfigNames)
                    {
                        PluginConfigs.Add(
                            new KeyValuePair<string, KeyValuePair<string, JObject>>(
                                kvp.Key,
                                new KeyValuePair<string, JObject>(kvp.Value, (JObject) data[kvp.Value])
                                ));
                    }
                }
                catch (Exception exc)
                {
                    this.SetupDone = true;

                    InitErrors.Add(string.Format("Problem reading config\n{0}", exc));

                    return null;
                }
            }
            this.SetupDone = true;
            return this;
        }
    }
}