define(['jquery', 'underscore', 'nvr', 'nvr_form', 'nvr_base64_url', 'nvr_ribbon', 'nvr_common'/*,...*/], function ($, _, NVR/*,...*/) {
	"use strict";
	
	NVR.helpdesk = {
		helpDeskDivideDialog: function(webUrl, ctid) {
			var newFormUrl = webUrl + "/_layouts/15/SPTools/NewFormContentType.aspx?ContentTypeId=" + ctid + "&FromID=" + NVR.Url.encode(window.location.href);
			NVR.form.openDialog(newFormUrl, function () { window.top.location.reload(true); });
			return false;
		},

		//depends on ContextGuids
		helpDeskEnableRibbonButtons: function(buttonCtid) {
			var ctid = ContextGuids.ctId;

			if (ctid == "0x010800004E0E57007A54000000000000000000") return true; //request
			if (ctid == "0x010800004E0E57007A54000011000000000000") return false; //task

			var wfctId = "0x010800D32E6121043E61498F48DC5711E35CA4";
			if (buttonCtid == wfctId && ctid == wfctId) return true;

			return false;
		},

		//depends on ContextGuids,Hlasky
		DuplicateItem: function() {
			$.ajax({
				"url": "/_layouts/15/SPTools/CopyListItem.aspx?Web=" + ContextGuids.web + "&SourceList=" + ContextGuids.list + "&ItemId=" + NVR.ribbon.GetSelectedItemsId()[0] + "&Duplicate=1&targetFolder=" + NVR.common.get_url_param("RootFolder"),
				"success": function (data, textStatus) {
					window.location.reload(true);
				},
				"error": function (XMLHttpRequest, textStatus, errorThrown) {
					//alert(XMLHttpRequest.responseText);
					alert(Hlasky["Error"]);
					window.location.reload(true);
				},
				"dataType": "text"
			});
		}

	};
	
	return NVR;
});