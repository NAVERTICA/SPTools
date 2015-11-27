define(['jquery', 'underscore', 'nvr', 'nvr_general', 'nvr_ribbon'/*,...*/], function ($, _, NVR/*,...*/) {
	"use strict";
	//depends on ContextGuids
	NVR.envelopes = {
		envelopeEnabled: function(type) {
			//type : 0=Serial  1=Parallel
			var id;
			if(NVR.general.isFormPage())
			{
			    if (NVR.general.isNewFormPage()) return false;
				id = ContextGuids.item.ID;
			}
			else
			{
			    if (!NVR.ribbon.EnableButtonOne()) return false;
				id = NVR.ribbon.GetSelectedItemsId()[0];
			}

			if (type == 0)
			{
			    var item = NVR.ribbon.GetItemsDataGetItemsData(id, "$select=NVR_EnvelopeLookup_Serial/Id&$expand=NVR_EnvelopeLookup_Serial");
				if(item == null) return false;
				if(item.NVR_EnvelopeLookup_Serial.Id != undefined) return false;
			}
			if (type == 1)
			{
			    var item = NVR.ribbon.GetItemsData(id, "$select=NVR_EnvelopeLookup_Parallel/Id&$expand=NVR_EnvelopeLookup_Parallel");
				if(item == null) return false;
				if(item.NVR_EnvelopeLookup_Parallel.Id != undefined) return false;
			}

			return true;
		},

		createEnvelope: function(type, listId) {
			if (ContextGuids.item != null)
			{
				itemId = ContextGuids.item.ID;
			}
			else
			{
			    var item = NVR.ribbon.GetItemsData(NVR.ribbon.GetSelectedItemsId()[0], "ID");
				itemId = item.ID;
			}

			SP.UI.ModalDialog.showWaitScreenWithNoClose('');//loading screen
			var url = "/_layouts/15/CScript.aspx?ConfigName=EnvelopeCreateScript&app=Template&wli=" + ContextGuids.web + ":" + listId + ":" + itemId + "&type=" + type;

			$.ajax(
			{
				"dataType": "text",
				"type": "POST",
				"url": url,
				"success": function (data) {
					SP.UI.ModalDialog.commonModalDialogClose(1, '');//loading screen close
					var currentLocation = window.location.href.split("&Source")[0];
					window.location.href = data + "&Source=" + currentLocation;
				},
				"error": function (XMLHttpRequest, textStatus, errorThrown) {
					SP.UI.ModalDialog.commonModalDialogClose(-1, '');//loading screen close
					alert(XMLHttpRequest.responseText);
				}
			});
		},

		enableReactiveEnvelope: function() {
			var item;
			if (ContextGuids.item != null)
			{
				item = ContextGuids.item;
			}
			else
			{
			    item = NVR.ribbon.GetItemsData(NVR.ribbon.GetSelectedItemsId()[0], "Status");
			}

			return ((item.Status == "Dokončeno") || (item.Status == "Completed")); //todo další jazyky
		},

		reactiveEnvelope: function(type, listId){

			var itemId;
			if (ContextGuids.item != null)
			{
				itemId = ContextGuids.item.ID;
			}
			else
			{
			    var item = NVR.ribbon.GetItemsData(NVR.ribbon.GetSelectedItemsId()[0], "$select=ID");
				itemId = item.ID;
			}

			SP.UI.ModalDialog.showWaitScreenWithNoClose('');//loading screen
			var url = "/_layouts/15/CScript.aspx?ConfigName=EnvelopeReactiveScript&app=Template&itemId=" + itemId;

			$.ajax(
			{
				"dataType": "text",
				"type": "POST",
				"url": url,
				"success": function (data) {
					SP.UI.ModalDialog.commonModalDialogClose(1, '');//loading screen close
					window.location.reload();
				},
				"error": function (XMLHttpRequest, textStatus, errorThrown) {
					SP.UI.ModalDialog.commonModalDialogClose(-1, '');//loading screen close
					alert(XMLHttpRequest.responseText);
				}
			});
		},
		
		//hromadny schvaleni/zamitnuti
		proccesEnvelope: function() {
			SP.UI.ModalDialog.showWaitScreenWithNoClose('');//loading screen
			var url = "/_layouts/15/CScript.aspx?ConfigName=EnvelopeAutomaticAction&app=Template&Ids&ids=" + NVR.ribbon.GetSelectedItemsId().join(",");

			$.ajax(
			{
				"dataType": "text",
				"type": "POST",
				"url": url,
				"success": function (data) {
					SP.UI.ModalDialog.commonModalDialogClose(1, '');//loading screen close
					window.location.reload();
				},
				"error": function (XMLHttpRequest, textStatus, errorThrown) {
					SP.UI.ModalDialog.commonModalDialogClose(-1, '');//loading screen close
					alert(XMLHttpRequest.responseText);
				}
			});
		}

	};
	
	return NVR;
});