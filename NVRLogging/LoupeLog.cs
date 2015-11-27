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
