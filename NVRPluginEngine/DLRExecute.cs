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
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.SharePoint;
using Navertica.SharePoint.Extensions;

namespace Navertica.SharePoint.RepoService
{
    /// <summary>
    /// Execute DLR scripts based on filetype and installed engines - IronPython for .py files included by default
    /// </summary>
    public class DLRExecute : IExecuteScript
    {
        private SPSite site;
        private bool _initialized = false;
        private ScriptRuntime runtime;

        public bool Initialized
        {
            get { return _initialized; }
        }

        public string[] FileExtensions()
        {
            return new[] { "*" };
        }

        public void InitEngine(SPSite site, List<string> fullPathsToAllFilesAndFolders)
        {
            this.site = site;
            runtime = (ScriptRuntime) HttpRuntime.Cache.Get("NVRDLRScriptRuntime");
            if (runtime == null)
            {
                runtime = CreateRuntime(fullPathsToAllFilesAndFolders);
                HttpRuntime.Cache.Insert("NVRDLRScriptRuntime", runtime);
            }
        }

        public ScriptContext Execute(string fullPath, FileAndDate scriptEditedAndBody, ScriptContext context)
        {
            ScriptEngine engine = runtime.GetEngineByFileExtension(fullPath.Split('.').Last().ToLowerInvariant());
            var paths = engine.GetSearchPaths();
            try
            {
                foreach (string path in
                    site.FeatureDefinitions[new Guid("867106ba-29b5-4301-8b78-d4597f31f22a")].Properties["Paths"]
                        .ToString().SplitByChars(";"))
                {
                    if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                    {
                        byte[] msg = ("Directory not found while trying to Paths from DLR feature: " + path).ToUtf8ByteArray();
                        engine.Runtime.IO.OutputStream.WriteAsync(msg, 0, msg.Count());
                        continue;
                    }
                    paths.Add(path);
                }
            }
            catch (Exception E)
            {
                byte[] exc = ("Exception while trying to load property Paths from DLR feature\n" + E.ToString()).ToUtf8ByteArray();
                engine.Runtime.IO.OutputStream.WriteAsync(exc, 0, exc.Count());
            }           
            engine.SetSearchPaths(paths);
            byte[] importPaths = string.Format("Import paths: {0}\n", paths.JoinStrings("; ")).ToUtf8ByteArray();
            engine.Runtime.IO.OutputStream.WriteAsync(importPaths, 0, importPaths.Count());
            CompiledCode code = Compile(fullPath, scriptEditedAndBody, engine);
            context.Add("site", this.site);
            // TODO cache scope with preloaded libraries and namespaces
            ScriptScope scope = engine.CreateScope(context);
            code.Execute(scope);
            return new ScriptContext(scope.GetItems());
        }

        private ScriptRuntime CreateRuntime(List<string> allScriptFullPaths)
        {
            var setup = Python.CreateRuntimeSetup(null);
            setup.HostType = typeof (SharePointAwareScriptHost);
            setup.HostArguments = new List<object> { allScriptFullPaths, this.site, @"c:\ironPythonLib\" };
            setup.Options["Frames"] = true;
            setup.Options["FullFrames"] = true;
            var runtime = new ScriptRuntime(setup);
            MemoryStream outputStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(outputStream);
            runtime.IO.SetOutput(outputStream, writer);
            runtime.IO.SetErrorOutput(outputStream, writer);

            return runtime;
        }
        
        private CompiledCode Compile(string scriptLink, FileAndDate scriptItem, ScriptEngine engine)
        {
            string cacheName = "script_" + scriptItem.LastUpdate.Ticks;
            KeyValuePair<DateTime, CompiledCode>? compiledItem = (KeyValuePair<DateTime, CompiledCode>?) HttpRuntime.Cache.Get(cacheName);

            // not in cache or not matching modificaction datetime of script from repository
            if (compiledItem == null || compiledItem.Value.Key != scriptItem.LastUpdate)
            {
                ScriptSource source = engine.CreateScriptSourceFromFile(scriptLink, Encoding.UTF8, SourceCodeKind.File);
                compiledItem = new KeyValuePair<DateTime, CompiledCode>(scriptItem.LastUpdate,
                    source.Compile());
                HttpRuntime.Cache.Insert(cacheName, compiledItem, null, DateTime.Now.AddDays(1), Cache.NoSlidingExpiration);
            }

            return compiledItem.Value.Value;
        }

        #region scripthost

        internal class SharePointAwarePlatformAdaptationLayer : PlatformAdaptationLayer
        {
            private List<string> _fullPathsToAllFilesAndFolders = new List<string>();
            internal SPSite Site;
            // this will be the same instance as the one in RepoServiceClient

            public string libraryLink = "";
            public static SharePointAwarePlatformAdaptationLayer PAL = new SharePointAwarePlatformAdaptationLayer();

            public List<string> FullPathsToAllFilesAndFolders
            {
                get { return _fullPathsToAllFilesAndFolders; }
                set { _fullPathsToAllFilesAndFolders = value; }
            }

            #region Private methods

            private Stream OpenResourceInputStream(string path)
            {
                string pathtofile = RepoServiceClient.NormalizedFilePath(path);
                if (FullPathsToAllFilesAndFolders.Contains(pathtofile))
                {
                    RepoServiceClient scriptrepo = new RepoServiceClient(Site);

                    var kvp = scriptrepo.Get(pathtofile);
                    if (kvp != null)
                        return new MemoryStream(kvp.BinaryData);
                }
                return null;
            }

            private bool LibraryDirectoryExists(string path)
            {
                bool result =
                    FullPathsToAllFilesAndFolders.Any(f => f.StartsWith(path.EndsWith("\\") ? path : path + "\\"));
                return result;
            }

            private bool LibraryFileExists(string path)
            {
                bool result = FullPathsToAllFilesAndFolders.Contains(path);
                return result;
            }

            public string GetLibraryLink()
            {
                string result = "";
                int partsCount = 0;
                foreach (string path in FullPathsToAllFilesAndFolders)
                {
                    int newPartsCount = path.Split('\\').Count();
                    if (partsCount == 0 || partsCount > newPartsCount)
                    {
                        partsCount = newPartsCount;
                        result = path;
                    }
                }
                return Path.GetDirectoryName(result);
            }

            #endregion

            #region Overrides from PlatformAdaptationLayer

            public override bool FileExists(string path)
            {
                path = RepoServiceClient.NormalizedFilePath(path);
                return LibraryFileExists(path) || base.FileExists(path);
            }

            public override string[] GetFileSystemEntries(string path, string searchPattern, bool includeFiles, bool includeDirectories)
            {
                string fullPath = RepoServiceClient.NormalizedFilePath(Path.Combine(path, searchPattern));

                if (FileExists(fullPath) || DirectoryExists(fullPath))
                {
                    return new[] { fullPath.Substring(2) };
                }
                return base.GetFileSystemEntries(fullPath, searchPattern, includeFiles, includeDirectories);
            }

            public override string GetFullPath(string path)
            {
                if (path.StartsWith(".\\")) 
                    return path;
                
                if (FileExists(path) || DirectoryExists(path))
                    return RepoServiceClient.NormalizedFilePath(path);

                return base.GetFullPath(path);
            }

            public override bool DirectoryExists(string path)
            {
                path = RepoServiceClient.NormalizedFilePath(path);
                return LibraryDirectoryExists(path) || base.DirectoryExists(path);
            }

            public override Stream OpenInputFileStream(string path)
            {
                return OpenResourceInputStream(path) ?? base.OpenInputFileStream(path);
            }

            public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share)
            {
                return OpenResourceInputStream(path) ?? base.OpenInputFileStream(path, mode, access, share);
            }

            public override Stream OpenInputFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
            {
                return OpenResourceInputStream(path) ?? base.OpenInputFileStream(path, mode, access, share, bufferSize);
            }

            #endregion
        }

        internal class SharePointAwareScriptHost : ScriptHost
        {
            public override PlatformAdaptationLayer PlatformAdaptationLayer
            {
                get { return SharePointAwarePlatformAdaptationLayer.PAL; }
            }

            public SharePointAwareScriptHost(object scriptFileStructure, SPSite site, string defaultDirectory)
            {
                SharePointAwarePlatformAdaptationLayer pal = ( (SharePointAwarePlatformAdaptationLayer) this.PlatformAdaptationLayer );
                pal.FullPathsToAllFilesAndFolders = (List<string>) scriptFileStructure;
                pal.Site = site;
                pal.CurrentDirectory = defaultDirectory;
                pal.libraryLink = pal.GetLibraryLink();
            }
        }

        #endregion
    }
}