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
using System.Web;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.SharePoint.Common.ServiceLocation;
using Microsoft.SharePoint;
using Navertica.SharePoint.Logging;
using Navertica.SharePoint.RepoService.Service;

namespace Navertica.SharePoint.RepoService
{
    /// <summary>
    /// This is the class that is accessible to the Client callers 
    /// (web parts, user controls, timer jobs, etc.). It just directs
    /// the calls through the <see cref="RepoWCFService"/> to the <see cref="RepoServiceApplication"/>
    /// </summary>
    public sealed class RepoServiceClient : BaseServiceClient
    {
        /// <summary>
        /// We need a live site reference to know where to load the scripts from
        /// </summary>
        public SPSite Site = null;

        private List<string> _fullPathsToAllFilesAndFolders = new List<string>();

        /// <summary>
        /// Relative paths to all files loaded from the SiteScripts libn
        /// </summary>
        public List<string> FullPathsToAllFilesAndFolders
        {
            get
            {
                if (_fullPathsToAllFilesAndFolders.Count == 0)
                {
                    this.ExecuteOnChannel<IRepoWCFService>(
                        delegate(IRepoWCFService channel)
                        {
                            _fullPathsToAllFilesAndFolders = channel.ListAllPaths(this.Site.Url);
                        },
                        false);
                }
                return _fullPathsToAllFilesAndFolders.OrderBy(s => s).ToList();
            }
            set { _fullPathsToAllFilesAndFolders = value; }
        }

        #region Methods

        /// <summary>
        /// Tells the service to load/reload current contents of the file (use relative url, root is in SiteScripts library root)
        /// </summary>
        /// <param name="filenamepath"></param>
        public void Reload(string filenamepath)
        {
            this.ExecuteOnChannel<IRepoWCFService>(
                delegate(IRepoWCFService channel)
                {
                    channel.Reload(this.Site.Url, filenamepath);
                },
                false);
        }

        /// <summary>
        /// Get contents and last modification date of specified file
        /// </summary>
        /// <param name="filenamepath">path and file (root is in SiteScripts library root)</param>
        /// <returns>File data and last update datetime in <see cref="FileAndDate"/></returns>
        public FileAndDate Get(string filenamepath)
        {
            FileAndDate response = null;

            this.ExecuteOnChannel<IRepoWCFService>(
                delegate(IRepoWCFService channel)
                {
                    response = channel.Get(this.Site.Url, filenamepath);
                },
                false);

            return response;
        }

        /// <summary>
        /// Normalize file path so it can be used in the RepoService
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static string NormalizedFilePath(string original)
        {
            string replacedSlash = original.Replace("/", "\\");

            if (replacedSlash.StartsWith(".\\"))
                return replacedSlash;

            if (replacedSlash.StartsWith("\\"))
                return "." + replacedSlash;

            if (replacedSlash == ".") return ".\\";

            return ".\\" + replacedSlash;
        }

        /// <summary>
        /// Loads the javascript file and surrounds it with script tag
        /// </summary>
        /// <param name="filenamepath"></param>
        /// <returns></returns>
        public string GetJavaScript(string filenamepath)
        {
            if (GetFiletype(filenamepath) != "js")
                return "<!-- not a javascript file: " + filenamepath + "-->\n";

            StringBuilder scriptsResult = new StringBuilder();
            FileAndDate scriptKvp = Get(filenamepath);
            scriptsResult.Append("<!-- JavaScript " + filenamepath + " start -->").AppendLine();
            if (scriptKvp != null)
            {
                scriptsResult.Append("<script type='text/javascript'>").AppendLine();
                scriptsResult.Append(scriptKvp.StringUnicode);
                scriptsResult.Append("</script>").AppendLine();
            }
            scriptsResult.Append("<!-- JavaScript " + filenamepath + " end -->\n").AppendLine();

            return scriptsResult.ToString();
        }

        /// <summary>
        /// Drop and reload all script data on site
        /// </summary>        
        public void Reset()
        {
            this.ExecuteOnChannel<IRepoWCFService>(
                delegate(IRepoWCFService channel)
                {
                    channel.Reset(this.Site.Url);
                },
                false);
        }

        /// <summary>
        /// Gets the Guid of IIS pool
        /// </summary>        
        public Guid ServiceApplicationIisGuid()
        {
            Guid r = Guid.Empty;
            this.ExecuteOnChannel<IRepoWCFService>(
                delegate(IRepoWCFService channel)
                {
                    r = channel.ServiceApplicationIisGuid();
                },
                false);
            return r;
        }

        /// <summary>
        /// Returns dict of filenames and <see cref="FileAndDate"/> with date of last change and file contents.
        /// </summary>
        /// <param name="subpaths">list of all the paths that should be included (relative to SiteScripts)</param>
        /// <returns>A dictionary of loaded files.</returns>
        public Dictionary<string, FileAndDate> GetAll(IEnumerable<string> subpaths)
        {
            Dictionary<string, FileAndDate> response = null;

            this.ExecuteOnChannel<IRepoWCFService>(
                delegate(IRepoWCFService channel)
                {
                    response = channel.GetAll(this.Site.Url, subpaths);
                },
                false);

            return response;
        }

        #endregion

        #region Script execution

        /// <summary>
        /// Executes a script specified by a path relative to SiteScripts, in the correct engine (DLR by default).
        /// Script is provided with a context that can be modified, usually with configuration and relevant SharePoint objects-
        /// </summary>
        /// <param name="scriptRelativePath"></param>
        /// <param name="context">Original context with variables</param>
        /// <returns>Resulting context with variables or, in case of error, a single item with its key starting with "_ERROR" 
        /// and the exception as value</returns>
        public ScriptContext Execute(string scriptRelativePath, ScriptContext context)
        {
            string fileextension = GetFiletype(scriptRelativePath);
            if (fileextension == string.Empty || scriptRelativePath == fileextension) return null;

            FileAndDate scriptItem = Get(scriptRelativePath);
             var log = NaverticaLog.GetInstance();

            if (scriptItem == null) return null;

            var filetypeMapping = AvailableExecuteScriptInterfaces();

            if (filetypeMapping.Count == 0)
            {
                log.LogError();
            }

            // find the right executor engine to execute this script
            IExecuteScript executorEngine;
            if (filetypeMapping.ContainsKey(fileextension))
                executorEngine = filetypeMapping[fileextension];
            else if (filetypeMapping.ContainsKey("*"))
                executorEngine = filetypeMapping["*"];
            else
                throw new Exception("Script Service Client - No execution interface for files with extension " + fileextension);

            if (!executorEngine.Initialized) executorEngine.InitEngine(this.Site, FullPathsToAllFilesAndFolders);

            // run the script with the original context and return the processed context            
            try
            {
                return executorEngine.Execute(scriptRelativePath, scriptItem, context);
            }
            catch (Exception e)
            {
                try
                {
                   
                    log.LogException(scriptRelativePath, e);
                }
                catch
                {
                    throw e;
                }
                return new ScriptContext() { { "_ERROR " + scriptRelativePath, e } };
            }
        }

        /// <summary>
        /// Uses SharePointServiceLocator to get available <see cref="IExecuteScript"/> interfaces
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, IExecuteScript> AvailableExecuteScriptInterfaces()
        {
            var filetypeMapping = (Dictionary<string, IExecuteScript>) HttpRuntime.Cache.Get("NVRExecuteScriptInterfaces");
            if (filetypeMapping == null || filetypeMapping.Count == 0)
            {
                // load the IExecuteScript interface for current file extension
                IServiceLocator serviceLocator = SharePointServiceLocator.GetCurrent(this.Site);
                IEnumerable<IExecuteScript> executeInterfaces = serviceLocator.GetAllInstances<IExecuteScript>();

                filetypeMapping = new Dictionary<string, IExecuteScript>();
                // load all the available interfaces
                try
                {
                    foreach (IExecuteScript execInterface in executeInterfaces)
                    {
                        string[] filetypes = execInterface.FileExtensions();
                        foreach (string filetype in filetypes)
                        {
                            filetypeMapping[filetype] = execInterface;
                        }
                    }
                }
                catch (Exception) {}
                HttpRuntime.Cache.Insert("NVRExecuteScriptInterfaces", filetypeMapping);
            }
            return filetypeMapping;
        }

        private static string GetFiletype(string filename)
        {
            return filename.Split('.').Last().ToLowerInvariant();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RepoServiceClient"/> class.
        /// </summary>
        [Obsolete]
        public RepoServiceClient() : base() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="RepoServiceClient"/> class.
        /// </summary>
        /// <param name="site"></param>
        public RepoServiceClient(SPSite site)
            : base(SPServiceContext.GetContext(site))
        {
            this.Site = site;
        }

        //public RepoServiceClient(string siteUrl) : this(SPServiceContext.GetContext(new SPSite(siteUrl))) {}

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the actual (.svc file) for this service.
        /// </summary>
        /// <remarks>
        /// Service applications are designed to support a single endpoint .svc file. For more complicated 
        /// service applications with many different types of services, it makes sense to create several .svc files 
        /// and classes. To support multiple end points, use a recognizable string here, and swap it out dynamically 
        /// in the BaseServiceClient's GetEndPoint method after the load balancer has provided the full path to this 
        /// original end point.
        /// </remarks>
        protected override string EndPoint
        {
            get { return "NVRRepoService.svc"; }
        }

        #endregion

    }
}