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
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Navertica.SharePoint.ConfigService
{
    /// <summary>
    /// Instantiated using ConfigBranch.GetInstance&lt;YourInheritedType&gt;(branchJsonString). Inherit this class for storing your configuration data in SiteConfig entries.  
    /// Use field accessors to read your config data in _data, which is automatically serialized, or define your own Serialize() and Initialize() methods.
    /// </summary>
    public class ConfigBranch
    {
        public class LanguageTitle
        {
            public string Language;
            public string Title;
        }

        [NonSerialized] private bool _debug;
        [NonSerialized] private string _json;
        [NonSerialized] private dynamic _data;
        [NonSerialized] protected bool SetupDone = false; // TODO - slo by pouzit pro zjistovani, jestli bylo na objektu provedeno Initialize
        [NonSerialized] public List<string> InitErrors = new List<string>();

        #region Constructors

        public ConfigBranch()
        {
            _json = "";
        }

        public ConfigBranch(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException();

            _json = json;
        }

        public ConfigBranch(ConfigBranch branch)
        {
            if (branch == null) throw new ArgumentNullException();
            _json = branch.Json;
        }

        #endregion

        #region Properties

        public bool Debug
        {
            get { return _debug; }
            set { _debug = value; }
        }

        public string Json
        {
            get { return _json; }
            set { _json = value; }
        }

        #endregion

        #region Virtual Metods

        // partial serialization / deserialization via http://stackoverflow.com/a/14527704/297606
        // http://www.west-wind.com/weblog/posts/2012/Aug/30/Using-JSONNET-for-dynamic-JSON-parsing
        public virtual string Serialize()
        {
            return JsonConvert.SerializeObject(this._data);
        }

        public virtual T Initialize<T>() where T : ConfigBranch
        {
            T result = JsonConvert.DeserializeObject<T>(_json);
            return result;
        }

        public virtual object Initialize()
        {
            _data = JsonConvert.DeserializeObject(_json);
            SetupDone = true;
            return this;
        }

        #endregion

        public static T GetInstance<T>(string json) where T : ConfigBranch
        {
            return GetInstance<T>(new ConfigBranch(json));
        }

        public static T GetInstance<T>(ConfigBranch branch) where T : ConfigBranch
        {
            var result = (T) branch.Initialize<T>();
            result.Json = branch.Json;
            result.SetupDone = true;
            return result;
        }

        public string ValidateJson(string schema)
        {
            return null; // TODO
        }
    }

   
}