using Microsoft.SharePoint;
using System;
using System.Collections.Generic;

namespace Navertica.SharePoint.RepoService
{
    /// <summary>
    /// Allows adding compilation/execution engines externally
    /// </summary>   
    public interface IExecuteScript
    {
        bool Initialized { get; }
        string[] FileExtensions();
        void InitEngine(SPSite site, List<string> fullPathsToAllFilesAndFolders);
        ScriptContext Execute(string fullPath, FileAndDate scriptEditedAndBody, ScriptContext context);
    }
}