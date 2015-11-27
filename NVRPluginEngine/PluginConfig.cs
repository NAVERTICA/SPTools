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