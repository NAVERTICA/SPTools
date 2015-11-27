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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Practices.SharePoint.Common.Logging;
using Microsoft.Practices.SharePoint.Common.ServiceLocation;
using Navertica.SharePoint.Interfaces;

namespace Navertica.SharePoint.Logging
{
    /// <summary>
    /// Custom logging class for logging to ULS and EventLog
    /// </summary>
    public class NaverticaLog : ILogging
    {
        readonly ILogger stdLog = SharePointServiceLocator.GetCurrent().GetInstance<ILogger>();
        
        public static ILogging GetInstance()
        {
            return SharePointServiceLocator.GetCurrent().GetInstance<ILogging>();
        }

        private string GetCallStack()
        {
            List<string> result = new List<string>();
            StackTrace st = new StackTrace();
            StackFrame[] stackFrames = st.GetFrames();
            foreach (var frame in stackFrames)
            {
                result.Add(frame.GetMethod().Module + " " + frame.GetMethod().Name + " " + frame.GetFileName() + " " + frame.GetFileLineNumber());
            }
            return string.Join("\n", result);
        }

        private string BuildMessage(params object[] args)
        {
            string message = "";

            if (!args.Any()) return "Empty message\n" + GetCallStack();

            if (args.Count() == 1)
            {
                message = (args[0] ?? "").ToString();
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                foreach (object arg in args)
                {
                    builder.Append(arg);
                    builder.Append(" ");
                }
                message = builder.ToString();
            }
            if (string.IsNullOrWhiteSpace(message)) message = "Empty message\n" + GetCallStack();
            return message;
        }

        public string LogInfo(params object[] args)
        {
            if (!args.Any()) return "Empty args\n" + GetCallStack();

            string message = BuildMessage(args);            

            stdLog.LogToOperations(message, "NAVERTICA/Info");

            return message;
        }

        public string LogWarning(params object[] args)
        {
            if (!args.Any()) return "Empty args\n" + GetCallStack();

            string message = BuildMessage(args);

            stdLog.LogToOperations(message, "NAVERTICA/Warn");

            return message;
        }

        public string LogError(params object[] args)
        {
            if (!args.Any()) return "Empty args\n" + GetCallStack();

            string message = BuildMessage(args);

            stdLog.LogToOperations(message, "NAVERTICA/Error");

            return message;
        }

        public string LogException(params object[] args)
        {
            if (!args.Any()) return "Empty args\n" + GetCallStack();

            string message = BuildMessage(args);

            stdLog.LogToOperations(message, "NAVERTICA/Error");

            return message;
        }


    }
}
