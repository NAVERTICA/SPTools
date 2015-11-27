import clr

clr.AddReference("Navertica.SPTools")
clr.AddReference("Navertica.SharePoint.Extensions")
clr.AddReference("NVRConfigService")
clr.AddReference("NVRLogging")
clr.AddReference("NVRPluginEngine")

from Navertica.SharePoint import Extensions as _ext
clr.ImportExtensions(_ext)

from Navertica.SharePoint import ExecutePagePluginHost, IPlugin
from Navertica.SharePoint.Logging import NaverticaLog

log = None

class TEST(IPlugin):
	def __init__(self):			print "TEST init"
	def Name(self):				return "TEST"
	def Description(self):		return "Makes sure we update the updated configuration in service"
	def Version(self):			return "0.2"
	def GetInstance(self):		
		scu = TEST()
		return scu
	def Test(self, config, context):
		return [(True, "Dummy test passed")]
	def Install(self, url, context):
		return "Install not needed"
	def x():
		print 1
		print 2
		print 3
	def Execute(self, config, context):        
		try:
			
			log.LogInfo("Running TEST.Execute")
			import wdb
			wdb.set_trace()
			print "trace"
			x()
			x()
			x()
			print "endtrace"
			#cl = RepoServiceClient(context.Site)
		
			# TODO sometime - config validation before updating, report errors and keep the working old config

			#cl.Reload(context.ListItem)

			return "TEST"
		except Exception, exc:
			log.LogError("TEST plugin failed with exception", exc)            

try:
	log = NaverticaLog.GetInstance()
	scu = TEST()
	print scu
	ExecutePagePluginHost.Add(site, scu)
except Exception, ee:
	log.LogError("TEST plugin failed to instantiate with exception", ee)