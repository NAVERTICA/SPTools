Navertica.SPTools 
===========
Enjoy the extensibility of SharePoint without having to deal with cumbersome deployment and misleading APIs
-----------

For on-premise SharePoint 2013, both Foundation and Server, using SSOM.

Alpha version, not for production.

Tl;dr
------
- Extend SharePoint using your favorite scripting languages (IronPython and other Dynamic Runtime Languages, runtime-compiled C# upcoming) - no need for Visual Studio for even the tiniest modifications
- Configure and activate all extensions from a single custom list with dynamically built form UI
- Develop, test and debug even on a production portal without the users noticing 

<table>
    <tr>
		<th>Activity</th>
		<th>SharePoint with SPTools</th>
		<th>Classic SharePoint</th>		
    </tr>
    <tr>
		<td>Development</td>
		<td>Use your favorite scripting language and environment or our [browser scripting console](http://www.navertica.com/en/Lists/News/DispForm.aspx?ID=23) - embedded editor upcoming. Save the file
		in our SiteScripts library, and it's done</td>
		<td>Use Visual Studio to write code, compile to DLL, deploy</td>		
    </tr>
    <tr>
		<td>Deployment</td>
		<td>Have changes deployed instantly after saving script</td>
		<td>Use command line/PowerShell scripts, reset farm after even the tiniest change</td>		
    </tr>
	<tr>
		<td>Configuration</td>
		<td>Based on JSON, centralized in a list, UI dynamically built using JSON schema and allowing dynamic selection of lists, fields etc.</td>
		<td>Saved in property bags, lists, databases - depending on the developer. User interface, if any, also depends on developer.</td>		
    </tr>
	<tr>
		<td>Activation</td>
		<td>Everything is activated by adding a configuration entry in our SiteConfig list, and will be immediately active for given URLs, content types, list types, etc. Moreover,
		configurations can be active only for certain people (developers, testers)</td>
		<td>Depends on the type of addon - some things can be deployed with XML on feature activation, some have to be manually added using lots of different screens, there's no one single place to look 
		to oversee it all</td>		
    </tr>
	<tr>
		<td>Testing</td>
		<td>Using dynamic languages and our extensions it's much easier to write easy-to-test code, and all tests can be run via our status page</td>
		<td>Possible but hard and hardly integrated</td>		
    </tr>
    <tr>
		<td>Debugging</td>
		<td>Add a debug statement to your script, open [wdb](https://github.com/Kozea/wdb) in another browser tab and step through your code in all comfort, 
		without blocking other users and operations (IronPython only, upcoming feature) </td>
		<td>Build and deploy a debug version of your assembly, attach Visual Studio to the right processes, block all other operations on the portal until debugging ends...</td>		
    </tr>	
</table>



Solution contents
==================

NVRConfigService
----------------
A simple SharePoint Service Application that holds configuration data and indexes. The configurations themselves are loaded
from our configuration list (SiteConfig). All functionalities query this service to find out what functionalities should be 
running where and with what configuration.

Only one instance of this service should be running per farm.

NVRRepoService
---------------
Second simple SharePoint Service Application - this one holds scripts that were uploaded to the SiteScripts library.
The NVRRepoServiceClient is also able to run scripts in different languages using engines that made themselves available through the
SharePoint Service Locator.

Only one instance of this service should be running per farm.

PageHead
--------------------------------
This control is added to every page and uses SharePoint Service Locator to run all registered IPageHead providers. Included are
JavaScript loaders, CSS loaders, dynamic ribbon buttons... and all of them use NVRConfigService
to find out what should be running in the page being rendered.

ItemReceiver (and ListReceiver, WebReceiver...)
-------------------------------------------
All of these just look to NVRConfigService what plugins are configured to run for given location and event type
and execute them. Currently these receivers have to be added manually, in the future the plugin install scripts will
be able to handle that.

GlobalItemReceiver
----------------------------------
These item receivers are targeted to content type 0x01, so they run on every item and every event everywhere. They don't 
do anything unless they find a configuration telling them they should. 

Execute page
-----------------------
Empty generic SharePoint page able to run plugins. The plugin can either treat it as an ASP.NET page, build, display and handle
controls, or it can be used as a simple backend.

Form Iterator
-----------------------
Our custom form iterator allows you to prefill values in forms with URL parameters (FieldInternalName=Value) and set some fields to
non-editable even in edit forms. 

Navertica.SharePoint.Extensions
-------------------------------
Extension methods for the Server Side Object Model that make a lot of clumsy API stuff simpler and allow hassle-free programming in dynamic scripting languages.
More details on https://github.com/NAVERTICA/Navertica.SharePoint.Extensions

IExecuteScript - scripting engine interface
-------------------------------------------
Used for making scripting engines available. Included implementations for DLR (IronPython, IronRuby, ...)
and DLL loader - loading classes from DLLs. Using PowerShell might also be possible.

IPlugin - plugin interface
--------------------------
A really simple interface for loading and running simple language independent plugin classes. To have your plugin automatically loaded,
it has to contain the string "plugin" in its filename. Once it is in SiteScripts, it should be possible to execute it by adding a
configuration entry and referring to it by its Name.


Required site structures 
==================
Will be automatically created when activating the NVRInfrastructure feature.

SiteConfig custom list
-------------------------
This list will be installed on activation of NVRInfrastructure if it doesn't exist. When installed for the first time, all rights will be removed
so that only admins can view and edit configurations. 

Properties:
- Title - free to use for anything
- Apps - application/plugin names this config entry adresses - one config entry can be used by several plugins, all of which will have their own sections
- Package - only used for grouping
- Order - several configs for one App name can be loaded, and the order of execution is set using this property
- Active - only entries marked as Active will be used
- Aproved - if an entry is not approved, it will load only for users in the Active For field
- Active For - users in this field will be able to load this config entry even if it is not Approved
- Debug - depending on the plugins and functionalities used, this can mean enhanced debug logging etc.
- URL
  - Can contain one or more relative URLs, each on its own line, for example ```/Lists/SiteConfig/*```
  - URLs can contain ```*``` and ```?``` as wildcards
  - If an URL pattern prefixed with minus ```-``` matches, this config entry will not be used even if other URL patterns/CT/ListType match
  - If empty, this configuration entry will depend on other properties to evaluate when to run
- CTs (Content types)
  - Can contain one or more ContentTypeIds, each on its own line, for example ```0x0108``` for all Tasks
  - can contain ```*``` and ```?``` as wildcards
  - if a pattern prefixed with minus ```-``` matches, this config entry will not be used even if other URL patterns/CT/ListType match
- List Type
  - Can contain one or more list types
- JSON
  - Use Default=1 in URL to edit the JSON contents as text.
  - Otherwise the field will look in SiteScripts/Schema for file with its internal name, in this case NVR_SiteConfigJSON.js
    This file contains a function that will construct a JSON schema and pass it to json-edit to dynamically build a form with it.

Configurations will be deemed active if 
- Active is true
- Approved is true or the current user is in the Active For field
- Every non-empty property matches - if URL, CT and LT properties are present, this config entry will only be included if it matches
  all three of them - it has to be on the right URL, the item has to have the right CT, and it has to be in the right LT. If a -prefixed possibility matches 
  in any one of the properties, the config entry will not be used.
	
SiteScripts library
-------------------------
This library will be installed on activation of NVRInfrastructure if it doesn't exist. The first time around, all rights will be removed
so that only admins can add and edit scripts. 

		
Installation 
===========
Order of installation: 

1) NVRLibraries.wsp
2) NVRLogging.wsp
3) NVRConfigService.wsp (install and instantiate service)
5) NVRRepoService.wsp (install and instantiate service)
7) NVRPluginEngine.wsp
8) NVRFormIterator.wsp
9) Navertica.SPTools.wsp


Installing Services 
========================
Only a single instance of each service should be running in a farm.

PowerShell scripts for the entire installation are upcoming.

1. NVRRepoServiceAdmin and NVRConfigServiceAdmin farm features must be installed and visible on (_admin/ManageFarmFeatures.aspx) 
2. In Central Administration Application Management page there will be new links to manage these services
3. Both need to be installed and run using (_admin/Server.aspx)
4. New instance for both service apps needs to be added, together with proxies (_admin/ServiceApplications.aspx)
5. Both proxies need to be associated with the web application where we want to use them - Service Application Associations (_admin/ApplicationAssociations.aspx)


Building addons using SPTools 
========

Not every enhancement and functionality has to be an addon, but for pieces that will be used frequently, it's worth doing it right.

A complete addon (as opposed to a plugin, which is just a piece of functionality loaded and used in the portal) is composed of several files that are copied into the SiteScripts 
library, and several files that are copied to SiteConfig, plus optional media files. The easiest way today to pack and distribute an addon is to put it in a .wsp,
which is kind of ironic, given how we boast there's no downtime to deploying our type of plugins, but we do have other options coming up.

The feature should contain two folders, SiteConfig and SiteScripts, which will be copied to _LAYOUTS/FeatureName and have to contain scripts/DLLplugin files for SiteScripts
and config files with JSON and metadata for SiteConfig.

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

The NVRAddAppEntriesIcon.js file looks like this and is configured using variables on top. It will redirect you to a SiteConfig NewForm,
and provided our custom NVRFormIterator is running, some form properties will be prefilled for you.
```js
(function() {
    if (!window["NVRAddAppEntries"]) window["NVRAddAppEntries"] = {};

    var appName = "DuplicateFields";
    var connectedApps = ["ItemUpdated", "ItemAdded"]; // []
    var packageName = "Receivers";
    var contentTypeId = "";
    var infoFilePath = "/NVR/DuplicateFields/Info.html";
    var minimalJSONconfig = '{"ItemUpdated": {"DuplicateFields--I": "DuplicateFields"},"ItemAdded": {"DuplicateFields--I": "DuplicateFields"} }';
	
	
    var relSource = NVR.getUrlParts(decodeURIComponent(tmpsrc)).pathname;
    
    window["NVRAddAppEntries"][appName] =
    {
        "Name": appName,
        "IconUrl": "/_layouts/15/images/ltdl.png?rev=23",
        "AppInstallUrl": "/Lists/SiteConfig/NewForm.aspx?"
            + "NVR_SiteConfigActive=true"
            + "&NVR_SiteConfigApp=" + appName + connectedApps.join("%0A") + "&Title=" + appName + "%20-%20/" + relSource
            + "&NVR_SiteConfigPackage=" + packageName
            + "&Source=" + NVR.common.get_url_param("Source")
            + "&NVR_SiteConfigUrl=/" + relSource
            + "&NVR_SiteConfigJSON=" + encodeURIComponent(minimalJSONconfig)
            + "&ContentTypeId=" + contentTypeId,
        "AppDetailsUrl": 'javascript:SP.UI.ModalDialog.showModalDialog({' +
            'url: "/_layouts/15/NVR.SPTools/GetScript.aspx?FilePaths=' + infoFilePath + '",' +
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