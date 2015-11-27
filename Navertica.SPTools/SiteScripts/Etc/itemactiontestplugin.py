import clr

clr.AddReference("Navertica.SPTools")
clr.AddReference("NVRPluginEngine")

from Navertica.SharePoint import ItemReceiverPluginHost, IPlugin

class DerivedAction(IPlugin):
	def __init__(self):		print "DerivedAction init"
	def Name(self):			return "DerivedAction"
	def Description(self):	return "Derived action description"
	def Version(self):		return "Derived action version"

	def Execute(self, config, context):
		return "Derived action executed with config\n" + (config or "NULL").ToString() + "\n" + (context or "NULL").ToString()

	def Test(self, config, context):
		return [(True, "Derived action test passed")]

	def GetInstance(self):
		da = DerivedAction()
		return da

	def Install(self, url, context):
		return "Install not needed"

da = DerivedAction()
print da
ItemReceiverPluginHost.Add(site, da)