using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Navertica.SharePoint.Interfaces;
using Navertica.SharePoint.Logging;
using Newtonsoft.Json;

namespace Navertica.SharePoint.ConfigService
{
    public class ItemReceiverPluginConfig : ConfigBranch
    {
        private bool? _eventFiringEnabled;
        
        /// <summary>
        /// Default true
        /// </summary>
        public bool? EventFiringEnabled
        {
            get { return _eventFiringEnabled; }
            set { _eventFiringEnabled = value; }
        }
    }

    public class TimerJobPluginConfig : ConfigBranch
    {
        private bool? _workingDaysOnly;

        /// <summary>
        /// Default true
        /// </summary>
        public bool? WorkingDaysOnly
        {
            get { return _workingDaysOnly; }
            set { _workingDaysOnly = value; }
        }

        public bool EnabledToRun
        {
            get
            {
                bool workDaysOnly = ( this.WorkingDaysOnly ?? true );

                if (workDaysOnly)
                {
                    int day = (int) DateTime.Now.DayOfWeek;
                    if (( day == 0 || day == 6 )) return false;
                }

                return true;
            }
        }
    }

    public class GenericConfigDictionary : ConfigBranch, IDictionary<string, string>
    {
        internal Dictionary<string, string> ConfigDict;

        public override string Serialize()
        {
            var tempConfigDict = new Dictionary<string, string>(ConfigDict);
            return JsonConvert.SerializeObject(tempConfigDict, Formatting.Indented);
        }

        public override object Initialize()
        {
            try
            {
                ConfigDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Json)
                             ?? new Dictionary<string, string>();
            }
            catch (Exception e)
            {
                ConfigDict = new Dictionary<string, string>();
                ILogging log = NaverticaLog.GetInstance();
                log.LogException(e);
            }
            return this;
        }

        public GenericConfigDictionary()
            : base()
        {
            ConfigDict = new Dictionary<string, string>();
        }

        public GenericConfigDictionary(string json) : base(json)
        {
            ConfigDict = new Dictionary<string, string>();
        }

        public GenericConfigDictionary(ConfigBranch branch) : base(branch) {}

        public GenericConfigDictionary(IDictionary<string, object> fromDict) : base(JsonConvert.SerializeObject(fromDict))
        {
            ConfigDict = new Dictionary<string, string>(fromDict.Keys.ToDictionary(s => ( fromDict[s] ?? "" ).ToString()));
        }

        #region dictionary interface

        public void Add(string key, string value)
        {
            ConfigDict.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return ConfigDict.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return ConfigDict.Keys; }
        }

        public bool Remove(string key)
        {
            return ConfigDict.Remove(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return ConfigDict.TryGetValue(key, out value);
        }

        public ICollection<string> Values
        {
            get { return ConfigDict.Values; }
        }

        public string this[string key]
        {
            get { return ConfigDict[key]; }
            set { ConfigDict[key] = value; }
        }

        public void Add(KeyValuePair<string, string> item)
        {
            ConfigDict.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            ConfigDict.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return ConfigDict.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return ConfigDict.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return ConfigDict.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return ConfigDict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ConfigDict.GetEnumerator();
        }

        #endregion
    }
}