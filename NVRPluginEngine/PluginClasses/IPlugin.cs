using System.Collections.Generic;
using Navertica.SharePoint.ConfigService;

namespace Navertica.SharePoint
{
    public interface IPlugin
    {
        string Name();
        string Description();
        string Version();
        List<PluginHost> PluginScope();
        object Execute(ConfigBranch config, object context);
        IList<KeyValuePair<bool, string>> Test(ConfigBranch config, object context);
        string Install(IEnumerable<string> url, IDictionary<string, object> context);
        IPlugin GetInstance();
    }
}