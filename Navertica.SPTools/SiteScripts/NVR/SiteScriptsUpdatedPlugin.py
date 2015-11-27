import clr

clr.AddReference("Navertica.SPTools")
clr.AddReference("Navertica.SharePoint.Extensions")
clr.AddReference("NVRRepoService")
clr.AddReference("NVRLogging")
clr.AddReference("NVRPluginEngine")

from System.Text.RegularExpressions import Regex, RegexOptions
from Navertica.SharePoint import Extensions as _ext
clr.ImportExtensions(_ext)

from Navertica.SharePoint import PluginHost, ItemReceiverPluginHost, IPlugin
from Navertica.SharePoint.RepoService import RepoServiceClient
from Navertica.SharePoint.Logging import NaverticaLog

log = None

class SiteScriptsUpdated(IPlugin):
    def __init__(self):				print "SiteScriptsUpdated init"
    def Name(self):					return "SiteScriptsUpdated"
    def Description(self):			return "Makes sure we update the updated script in service"
    def Version(self):				return "0.1"

    def Execute(self, config, context):
        try:
            if not context.ListItem: return

            log.LogInfo(str.Format("Running SiteScriptsUpdated.Execute on {0}\nserver relative url is {1}", context.ListItem.FormUrlDisplay(), None)) #context.ListItem.File.ServerRelativeUrl))

            # TODO check content type, after we've started to use it
            cl = RepoServiceClient(context.Site)
            # TODO get file content and write it into Script field

            # TODO sometime - config validation before updating, report errors and keep the working old config
            scriptPath = Regex.Replace(context.ListItem.File.ServerRelativeUrl, "^[\/]?SiteScripts\/\s*", "", RegexOptions.Multiline)

            cl.Reload(scriptPath)

            log.LogInfo("SiteScriptsUpdated", PluginHost.Reload(scriptPath, context.Site))

            return "Reloaded SiteScripts item\n" + context.ListItem.FormUrlDisplay()
        except Exception, exc:
            log.LogError("SiteScriptsUpdated plugin failed with exception", exc)

    def GetInstance(self):
        ssc = SiteScriptsUpdated()
        return ssc

    def Test(self, config, context):
        return [(True, "Dummy test passed")]

    def Install(self, url, context):
        return "Installed automatically"


try:
    log = NaverticaLog.GetInstance()
    ssc = SiteScriptsUpdated()
    print ssc
    ItemReceiverPluginHost.Add(site, ssc, True)
    log.LogInfo("SiteScriptsUpdated instantiated", ", ".join(ItemReceiverPluginHost.List(site)))
except Exception, ee:
    log.LogError("SiteScriptsUpdated plugin failed to instantiate with exception", ee)