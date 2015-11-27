import clr

clr.AddReference("Navertica.SPTools")
clr.AddReference("NVRPluginEngine")

from Navertica.SharePoint import ExecutePagePluginHost, IPlugin

class ExecuteScriptTest(IPlugin):
	def __init__(self):		print "ExecuteScriptTest init"
	def Name(self):			return "ExecuteScriptTest"
	def Description(self):	return "ExecuteScriptTest description"
	def Version(self):		return "ExecuteScriptTest version"

	def Execute(self, config, context):
		context["Response"].Write("ExecuteScriptPlugin executed")

	def Install(self, urls, context):
		return "Installed automatically"

	def Test(self, config, context):
		return [(True, "ExecuteScript action test passed")]

	def GetInstance(self):
		est = ExecuteScriptTest()
		return est

est = ExecuteScriptTest()
print est
ExecutePagePluginHost.Add(site, est)