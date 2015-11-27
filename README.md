Navertica.SPTools (alpha version, not for production)
===========

For on-premise SharePoint 2013, both Foundation and Server.

Our tools introduce new possibilities to extending SharePoint:
- Use DLR languages like IronPython in event receivers, timerjobs, pages...
- Plugins can be created using DLR languages as well as C# and deployed in farm *without downtime*
- All the scripts, plugins and JavaScript enhancements are stored in a single place, the SiteScripts library
- Configurations for all the plugins are also stored in a single place, the SiteConfig custom list, in our custom JSONField
- System knows where different plugins should run via properties (like URL, ContentType...) of each configuration entry. 
  It's possible to use wildcards and exclude certain patterns in these properties, so it's very easy to for example:
  - include a JS mod on every EditForm in a subweb by setting the URL of its SiteConfig entry to /subweb/*/EditForm.aspx
  - have a receiver run on every item with a content type by leaving URL blank and setting CT property to 0x0108...
- To make things easier, the Add an App menu is extended with icons of installed plugins, which redirect to and prefill a new SiteConfig entry
- The custom JSONField in SiteConfig stores configuration data as JSON. 
  It can work as a simple text editor (add parameter Default=1 to URL), but it's able to construct complex forms 
  that allow configurating to be interactive and safe by letting you choose lists, fields, etc. instead of typing down
  their names. 
- Configuration entries can be enabled only for certain users, so it's possible to develop, test and deploy plugins and 
  modifications without anyone noticing.

With SPTools, it's possible to develop, comfortably test and even debug (upcoming) functionalities right on server, 
or even on production - without disturbing users and with no need for Visual Studio or additional tools.


Solution contents
==================

Navertica.SharePoint.Extensions
-------------------------------
Extensions to SSOM that make a lot of clumsy API stuff simpler and allow hassle-free programming in untyped scripting languages.
More details on https://github.com/NAVERTICA/Navertica.SharePoint.Extensions

NVRConfigService
----------------
A simple SharePoint Service Application that holds configuration data and indexes. The configurations themselves are loaded
from our configuration list (SiteConfig). 

Only one instance of this service should be running per farm.

NVRRepoService
---------------
Second simple SharePoint Service Application - this one holds scripts that were uploaded to the SiteScripts library.
The NVRRepoServiceClient is also able to run scripts in different languages using engines that made themselves available through the
SharePoint Service Locator.

Only one instance of this service should be running per farm.

PageHead - a control added to every page
--------------------------------
Uses SharePoint Service Locator to run all registered IPageHead providers. Included are
JavaScript loaders, CSS loaders, dynamic ribbon buttons... and all of them use NVRConfigService
to find out what should be running in the page being rendered.

ItemReceiver (and ListReceiver, WebReceiver...)
-------------------------------------------
All of these just look to NVRConfigService what plugins are configured to run for given location and event type
and execute them. Currently these receivers have to be added to lists by hand.

GlobalItemReceiver
----------------------------------
These receivers are targeted to content type 0x01, so they run on every item and every event everywhere. They don't 
do anything unless they find a configuration telling them they should.

Execute page
-----------------------
Empty generic SharePoint page able to run plugins. The plugin can either treat it as an ASP.NET page, build, display and handle
controls, or it can be used as a simple backend.

IExecuteScript - scripting engine interface
-------------------------------------------
Used for making scripting engines available. Included implementations for DLR (IronPython, IronRuby, ...)
and DLL loader - loading classes from DLLs. Using PowerShell would be also nice, but we haven't researched it yet.

IPlugin - plugin interface
--------------------------
A really simple interface for loading and running simple language independent plugin classes.


SiteConfig custom list
-------------------------
This list will be installed on activation of NVRInfrastructure if it doesn't exist. The first time around, all rights will be removed
so that only admins can view and edit configurations. 

Properties:
- ...
- JSON
  - Use Default=1 in URL to edit the JSON contents as text.
  - Otherwise the field will look in SiteScripts/Schema for file with its internal name, in this case NVR_SiteConfigJSON.js
    This file contains a function that will construct a JSON schema and pass it to json-edit to dynamically build a form with it.
	

Installation (to be continued)
===========
Order of installation (solutions are not currently stapled): 

1) NVRLibraries
2) NVRLogging
3) NVRConfigService
5) NVRRepoService
7) NVRPluginEngine
8) Navertica.SPTools


Installing Services (to be continued)
========================
Only a single instance of each service should be running in a farm.

PowerShell scripts for the entire installation are being prepared.

1. NVRRepoServiceAdmin and NVRConfigServiceAdmin farm features must be installed and visible on (_admin/ManageFarmFeatures.aspx) 
   - If they're not visible, they need to be installed
   - STSADM.EXE -o installfeature -name NVRConfigService_NVRConfigServiceAdmin
   - STSADM.EXE -o activatefeature -name NVRConfigService_NVRConfigServiceAdmin
   - STSADM.EXE -o installfeature -name NVRRepoService_RepoServiceAdmin
   - STSADM.EXE -o activatefeature -name NVRRepoService_RepoServiceAdmin
2. In Central Administration Application Management page there will be new links to manage these services
3. Both need to be installed and run using (_admin/Server.aspx)
4. New instance for both service apps needs to be added, together with proxies (_admin/ServiceApplications.aspx)
5. Both proxies need to be associated with the web application where we want to use them - Service Application Associations (_admin/ApplicationAssociations.aspx)


Building addons using SPTools (to be continued)
========

Not every enhancement and functionality has to be an addon, but for pieces that will be used frequently, it's worth doing it right.

A complete addon (as opposed to a plugin, which is just a piece of functionality) is composed of several files that are copied into the SiteScripts 
library, and several files that are copied to SiteConfig, plus optional media files. The easiest way today to pack and distribute an addon is to put it in a .wsp,
which we know is kind of ironic, given how we boast there's no downtime to deploying our type of plugins, but we do have some other options coming up.

The feature should contains two folders, SiteConfig and SiteScripts, which will be copied to _LAYOUTS/FeatureName and have to contain scripts/DLLs for SiteScripts
and config files for SiteConfig.

In order to be visible in the Add an App page, the addon has to provide SiteConfig configuration with these two files:

Filename: FeatureName.metadata.json

Contents:
```json
{
  "ContentType": "NVR_SiteConfigJSON",
  "NVR_SiteConfigApp": "JavaScripts",
  "NVR_SiteConfigPackage": "YOUR_CUSTOM_PACKAGE_NAME",
  "NVR_SiteConfigListType": null,
  "NVR_SiteConfigUrl": "*/_layouts/15/addanapp.aspx*",
  "NVR_SiteConfigOrder": 1,  
  "NVR_SiteConfigActive": true,
  "NVR_SiteConfigApproved": true,
  "Title": "Add YOUR_FEATURE into 'Add an App' page"
}
```

Filename: FeatureName.json

Contents:
```json
{
    "JavaScripts": {
      "LibLinks": [
        ""
      ],
      "ScriptLinks": [
        "YOUR_FOLDER_UNDER_SITESCRIPTS/YOUR_FEATURE_NAME/NVRAddAppEntriesIcon.js"
      ]
    }
}
```

In the SiteScripts folder, you have the NVRAddAppEntriesIcon.js and on the same level there is Info.html, which contains the information
to be displayed on the Add an App page details.

The NVRAddAppEntriesIcon.js file looks like this:
```js
(function() {
    if (!window["NVRAddAppEntries"]) window["NVRAddAppEntries"] = {};
	
	// configure these
	var YOUR_FEATURE_NAME = "YourFeatureName";
	var YOUR_PACKAGE = "YourPackageName";
	var FEATURE_INFO_PATH = "/YOUR_FOLDER_UNDER_SITESCRIPTS/YOUR_FEATURE_NAME/Info.html";
	

    function getUrlParts(url) {
        ...
    }

    var tmpsrc = get_url_param("Source");
    var relSource = getUrlParts(decodeURIComponent(tmpsrc)).pathname;
    console.log("relSource", relSource);

    window["NVRAddAppEntries"]["DuplicateFields"] =
    {
        "Name": "DuplicateFields",
        "IconUrl": "/_layouts/15/images/ltdl.png?rev=23",
        "AppInstallUrl": "/Lists/SiteConfig/NewForm.aspx?"
            + "NVR_SiteConfigActive=true&NVR_SiteConfigActiveFor=" + encodeURIComponent(ContextGuids.userlogin) 
            + "&NVR_SiteConfigApp=" + YOUR_FEATURE_NAME + "--I%0AItemUpdated%0AItemAdded&Title=" + YOUR_FEATURE_NAME + "%20-%20/" + relSource
            + "&NVR_SiteConfigPackage=" + YOUR_PACKAGE
            + "&Source=" + tmpsrc
            + "&NVR_SiteConfigUrl=/" + relSource
            + "&NVR_SiteConfigJSON=" + encodeURIComponent('{"ItemUpdated": {"' + YOUR_FEATURE_NAME '"--I": "' + YOUR_FEATURE_NAME + '"},"ItemAdded": {"' + YOUR_FEATURE_NAME + '--I": "' + YOUR_FEATURE_NAME + '"} }')
            + "&ContentTypeId=0x010000AF00A19FBA41D68E7FDB37BCB1847B",
        "AppDetailsUrl": 'javascript:SP.UI.ModalDialog.showModalDialog({' +
            'url: "/_layouts/15/SPTools2013/GetScript.aspx?FilePaths=' + FEATURE_INFO_PATH + '",' +
            'width: 800,' +
            'height: 600,' +
            'allowMaximize: true});'
    };
})();
```

The feature in the .wsp should have a feature event receiver that is inherited from Navertica.SharePoint.NVRFeatureReceiver.

The feature also has to have two custom properties defined, NVR_SiteScriptsFolder and NVR_SiteConfigFolder, where the relative way to the 
_LAYOUTS/YOUR_FEATURE_FOLDER/SiteConfig and SiteScripts is stored, so that on activation all the files get copied to SiteConfig and SiteScripts.

Copyright (C) 2015 NAVERTICA a.s. http://www.navertica.com 

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.