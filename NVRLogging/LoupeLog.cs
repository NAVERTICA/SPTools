using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Microsoft.Practices.SharePoint.Common.Logging;
//using Microsoft.Practices.SharePoint.Common.ServiceLocation;
using Gibraltar.Agent;
using Navertica.SharePoint.Interfaces;

namespace Navertica.SharePoint.Logging
{
    /// <summary>
    /// Logging to Loupe (and EventLog and ULS)
    /// </summary>
    public class LoupeLog : NaverticaLog
    {

        public new string  LogInfo(params object[] args)
        {
            string message = base.LogInfo(args);
            
            Log.Information("info", "info", message, args);

            return message;
        }

        public new string LogWarning(params object[] args)
        {
            string message = base.LogWarning(args);

            Log.Warning("warning", "warning", message, args);

            return message;

        }

        public new string LogError(params object[] args)
        {
            string message = base.LogError(args);

            Log.Error("error", "error", message, args);

            return message;
        }

        public new string LogException(params object[] args)
        {
            string message = base.LogException(args);
            if (args != null && args.Length > 0 && args[0] is Exception)
            {
                Log.RecordException((Exception)args[0],"exception",true);
            }

            Log.Warning("exception", "exception", message, args);

            return message;
        }


    }
}
