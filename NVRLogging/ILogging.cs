using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Navertica.SharePoint.Interfaces
{
    public interface ILogging
    {
        string LogInfo(params object[] args);
        string LogWarning(params object[] args);
        string LogError(params object[] args);
        string LogException(params object[] args);        
    }
}
