import clr

clr.AddReference("Navertica.SharePoint.Extensions")
clr.AddReference("NVRConfigService")
clr.AddReference("NVRLogging")
clr.AddReference("NVRPluginEngine")
clr.AddReferenceToFileAndPath(r"c:\Program Files\Common Files\Microsoft Shared\web server extensions\15\ISAPI\Microsoft.SharePoint.dll")

from Microsoft.SharePoint import *
from Navertica.SharePoint import Extensions as _ext
clr.ImportExtensions(_ext)

from Navertica.SharePoint import PluginHost, ItemReceiverPluginHost, IPlugin
from Navertica.SharePoint.ConfigService import ConfigServiceClient
from Navertica.SharePoint.Logging import NaverticaLog

log = None
installData = {}

class SiteConfigUpdated(IPlugin):
    def __init__(self):			print "SiteConfigUpdated init"
    def Name(self):				return "SiteConfigUpdated"
    def Description(self):		return "Makes sure we update the updated configuration in service"
    def Version(self):			return "0.1"

    def Execute(self, config, context):
        try:
            if context == None: return

            item = None
            site = None
            if type(context) is SPListItem:
				item = context
				site = item.Web.Site
            if type(context) is SPItemEventProperties:
				item = context.ListItem
				site = context.Site

            installData["site"] = site

            #log.LogInfo(item, site)

            log.LogInfo("Running SiteConfigUpdated.Execute on " + item.FormUrlDisplay())
            cl = ConfigServiceClient(site)

            # TODO sometime - config validation before updating, report errors and keep the working old config

            cl.Reload(item)

            try:
                # try to load apps + run their Install
                self.installAppsAndUrls(site, item)
            except Exception, ee:
                return "Reloaded SiteConfig item, failed installing apps\n" + item.FormUrlDisplay() + "\n" + ee.ToString()

            return "Reloaded SiteConfig item\n" + item.FormUrlDisplay()

        except Exception, exc:
            return log.LogError("SiteConfigUpdated plugin failed with exception", exc)

    def installAppsAndUrls(self, site, item):
        #log.LogInfo("start installAppsAndUrls")
        apps = list([app.split("--")[0] for app in item["NVR_SiteConfigApp"].SplitByChars(",\n")])
        urls = list(item["NVR_SiteConfigUrl"].SplitByChars("\n"))
        #log.LogInfo(apps, urls)

        #print apps
        #print urls

        for app in apps:
            try:
                #print app
                #print site
                plugin = PluginHost.Get(site, app)
                if plugin:
                    plugin.Install(urls, installData)
            except Exception, ae:
                print ae
                log.LogError(str.Format("SiteConfigUpdated exception trying to Install plugin {0} - exception {1} "), app, ae)
                continue

		# kontrola na zmenene verze
		#if len(item.Versions) == 1:
        #    self.installApps(context, apps, urls)

        #oldapps = list([app.Split("|")[0] for app in context.ListItem.Versions[1].ListItem["NVR_SiteConfigApp"].SplitByChars(",\n")])
        #oldurls = list(context.ListItem.Versions[1].ListItem["NVR_SiteConfigUrl"].SplitByChars("\n"))

        #addedapps = list(set(oldapps) - set(apps))
        #addedurls = list(set(oldurls) - set(urls))

        #log.LogInfo("SiteConfigUpdated added apps\n" + ", ".join(addedapps) + "\nand urls\n" + ", ".join(addedurls))
        #if addedapps:
        #    self.installApps(context, addedapps, urls)
        #if addedurls:
        #    self.installApps(context, apps, addedurls)

        #self.installApps(context, apps, addedurls)

    def GetInstance(self):
        scu = SiteConfigUpdated()
        return scu

    def Install(self, urls, context):
        return "Installed automatically"

    def Test(self, config, context):
        return [(True, "Dummy test passed")]

try:
    log = NaverticaLog.GetInstance()
    scu = SiteConfigUpdated()
    #print scu
    #PluginHost.Reset()
    PluginHost.Init(site)
    #print PluginHost.List(site)
    ItemReceiverPluginHost.Add(site, scu, True)

    #with site.OpenWeb("") as web:
    #    l = web.OpenList("SiteConfig")
    #    item = l.GetItemById(248)
    #    scu.Execute(None, item)
except Exception, ee:
    log.LogError("SiteConfigUpdated plugin failed to instantiate with exception", ee)