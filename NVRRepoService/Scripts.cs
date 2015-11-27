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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SharePoint;
using Navertica.SharePoint.Config;
using Navertica.SharePoint.Config.Service;
using Navertica.SharePoint.Interfaces;
using Pathoschild.DesignByContract;
using Newtonsoft.Json;
using Navertica.SharePoint.Extensions;
using PGK.Extensions;
using StructureMap;


namespace Navertica.SharePoint.Scripting
{
    public class ScriptContext : DictionaryNVR
    {
        public ScriptContext(IEnumerable<KeyValuePair<string, object>> d)
        {
            foreach(KeyValuePair<string, object> kvp in d)
            {
                this[kvp.Key] = kvp.Value;
            }
        }

        public ScriptContext()
        {            
        }
    };

    /// <summary>
    /// 
    /// </summary>
    public class ScriptsBackup
    {
        private Dictionary<string, string> _files;
        public Dictionary<string, string> Files { get { return _files; } }
        private static Dictionary<string, IScriptEngine> _engines = new Dictionary<string, IScriptEngine>(); 

        public void LoadScripts(SPSite site)
        {
            if (site == null) return;
            _files = LoadAllScripts(site);
            ConfigurationsServiceClient configs = new ConfigurationsServiceClient();
            List<string> dllFullNameAndPath = new List<string>();

            // from SiteConfig we load all entries with App set to "ScriptEngine"
            foreach (var cfgid in configs.Filter(site.RootWeb, new Dictionary<ConfigProperties, string>() {{ ConfigProperties.App, "ScriptEngine"}}))
            {
                GenericConfigDictionary cfg = (GenericConfigDictionary)configs.GetBranch<GenericConfigDictionary>(cfgid, "Scripts");
                
                // if the configuration entry is a dictionary and contains IScriptEngineAssemblyPath
                // we add it to the DLL list
                if (cfg != null && cfg.ContainsKey("IScriptEngineAssemblyPath"))
                {
                    string path = (cfg["IScriptEngineAssemblyPath"] ?? "").ToString();
                    if (!string.IsNullOrEmpty(path)) dllFullNameAndPath.AddUnique(path);
                }
            }

            // load the DLLs
            foreach (string dllFullName in dllFullNameAndPath)
            {
                ObjectFactory.Configure(x => x.Scan(scan => scan.Assembly(dllFullName)));
            }

            // get all the classes implementing IScriptEngine
            ObjectFactory.Configure(x => x.Scan(scan => scan.AddAllTypesOf<IScriptEngine>()));

            // instantiate all engines and save the references in the singleton Scripts object
            foreach (IScriptEngine engine in ObjectFactory.GetAllInstances<IScriptEngine>())
            {
                foreach (string filetype in engine.FileTypes())
                {
                    _engines[filetype.ToLowerInvariant()] = engine;
                }
            }
        }
        
        /// <summary>
        /// Used to get the only valid instance    
        /// </summary>
        /// <param name="site">Used to load engine configuration and all scripts if the class needs to be constructed</param>
        /// <returns>Singleton instance of Scripts</returns>
        /*public static Scripts GetInstance()
        {
            // TODO
            return null; // ObjectFactory.GetInstance<Scripts>();
        }*/

        private string GetFilename([NotNull] string fn)
        {
            string filename = fn;
            if (filename.Contains("SiteScripts/"))
                filename = filename.Split(new[] { "SiteScripts/" }, 2, StringSplitOptions.RemoveEmptyEntries).Last();
            else if (filename.Contains("SiteScripts\\"))
                filename = filename.Split(new[] { "SiteScripts\\" }, 2, StringSplitOptions.RemoveEmptyEntries).Last();
            return filename;
        }
        
        private string GetFiletype(string filename)
        {
            return filename.Split('.').Last().ToLowerInvariant();
        }

        public ScriptContext Run([NotNull] string siteScriptsFilename, [NotNull] IDictionary<string, object> givenContext)
        {
            ScriptContext context = new ScriptContext(givenContext);
            string filename = GetFilename(siteScriptsFilename.Replace("/", "\\"));
            string filetype = GetFiletype(filename);           

            // load script text
            string script = Files[filename];

            // load engine            
            if (!_engines.ContainsKey(filetype)) throw new ArgumentException("No engine to process filetype " + filetype);
            IScriptEngine engine = _engines[filetype];

            // run in engine
            context = new ScriptContext(engine.RunScript(script, context));

            // return resulting context
            return context;
        }


        public ScriptContext Run(string[] siteScriptsFilenames, IDictionary<string, object> givenContext)
        {
            ScriptContext context = new ScriptContext(givenContext);
            foreach (string fn in siteScriptsFilenames)
            {
                foreach (KeyValuePair<string, object> kvp in Run(fn.Replace("/", "\\"), context))
                {
                    context[kvp.Key] = kvp.Value;
                }
            }
            return context;
        }

        public string GetContent([NotNull] string siteScriptsFilename)
        {
            string result = "";
            string filename = GetFilename(siteScriptsFilename.Replace("/", "\\"));
            if (Files.ContainsKey(filename)) result = Files[filename];
            return result;
        }

        public string RemoveMatchingStringFromStart(string removeFrom, [NotNull, NotBlank] string toMatch)
        {
            if (removeFrom == null || toMatch == null) throw new ArgumentNullException();
            int position = 0;
            foreach (char ch in toMatch)
            {
                if (removeFrom[position] != ch) break;
                position++;
            }
            return removeFrom.Substring(position);
        }

        private Dictionary<string, string> LoadAllScripts(SPSite site)
        {
            if (site == null) throw new ArgumentNullException("site");
            SPWeb rootWeb = site.RootWeb;

            Dictionary<string, string> output = new Dictionary<string, string>();
            SPList sList = rootWeb.OpenList("SiteScripts", true);

            sList.ProcessItems(delegate(SPListItem script)
            {
                string relativePath = RemoveMatchingStringFromStart(script.DocumentUrl(), sList.AbsoluteUrl()).Replace("/", "\\");
                if (!output.ContainsKey(relativePath))
                {
                    output.Add(script.IsFolder() ? relativePath + "\\" : relativePath,
                               script.IsFolder() ? null : (string)script["Script"]);
                }

                return null;
            });

            return output;
        }

        public Dictionary<string, string> GetScriptsWithExtension(Dictionary<string, string> scripts, string extension)
        {            
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> kvp in scripts.Where(kvp => kvp.Key.ToLowerInvariant().EndsWith("." + extension)).Where(kvp => !output.ContainsKey(kvp.Key)))
            {
                output.Add(kvp.Key, kvp.Value);
            }

            return output;
        }

    }

  
}
