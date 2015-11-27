define(['jquery', 'underscore', 'nvr', 'nvr_e_n_d_form', 'nvr_general', 'nvr_common', 'nvr_base64_url'/*,...*/], function ($, _, NVR/*,...*/) {
	"use strict";
	
	NVR.form = {
		getEditFormVal: function(cellName) {
			// hodnota ovládacího prvku pro řádek s daným display name
		    var editForm = NVR.e_n_d_form.getFormBody(cellName);

			if (editForm.length == 0) {
				return null;
			}

			var elem = editForm.find("input");

			if (elem.length > 0) {
				if ($(elem[0]).attr("type") == "checkbox") {
					if ($(elem[0]).attr("checked"))
						return true;
					else
						return false;
				}
				if ($(elem[0]).attr("type") == "radio") {

				}

				return $(elem).val();
			}

			elem = editForm.find("textarea");

			if (elem.length > 0) {
				return $(elem).val();
			}

			elem = editForm.find("select");

			// neosetreny multilookup
			if (typeOf(elem[0].options) != "undefined") {
				return elem[0].options[elem[0].selectedIndex].text;
			}

			// neosetrene checkboxy
			return null;
		},

		loadManagerIntoField: function(fieldDisplayName) {
			$.ajax({
				"url": "/_layouts/15/SPTools/GetManager.aspx",
				"success": function (data, textStatus) {
					putValueInInput(fieldDisplayName, data, true);
				},
				"error": function (XMLHttpRequest, textStatus, errorThrown) {
					reportError("Nepodařilo se předvyplnit manažera do pole " + fieldDisplayName);
				},
				"dataType": "text"
			});
		},

	    //depends on ContextGuids
		loadCurrentUserIntoField: function(fieldDisplayName) {  
			putValueInInput(fieldDisplayName, ContextGuids.userlogin, true);
		},

		// ocistit form pro tisk
		clearFormPage: function(variant) {
			// pred appendem je potreba zamezit <script> tagum v tom, aby se znovu spustily
			if (variant && variant == 2) {
				$('<div style="padding-left: 10px; padding-right: 10px; width: 95%" class="ms-descriptiontext" ID="formAppendPoint"></div>').appendTo("#aspnetForm");
			} else {
				$('<div ID="formAppendPoint"></disv>').appendTo("form");
			}

			$(".ms-formtable script").remove();
			$(".akh__popup").remove();
			$(".ms-formtable").appendTo("#formAppendPoint");
			$("body").css("overflow", "scroll");
			$("form").children("[id!=formAppendPoint]").hide(); // SP2010
		},

		CheckFilled: function(fieldDisplayName) { 
			// je ve form nějaká vyplněná hodnota v řádku daného displayname?
			var result = null;
			try {
				result = this.getEditFormVal(fieldDisplayName);
			} catch (err) {

			}

			if (result && (result == "true" || result == "")) {
				return false;
			}

			return true;
		},

		getDateFromSPField: function(fieldDisplayName) { 
			var dateJsParseAttemptDate = Date.parse(this.getEditFormVal(fieldDisplayName));
			if (dateJsParseAttemptDate)
				return dateJsParseAttemptDate;
			else
				return null;
		},

		// TODO predelat tak, aby to ve stare strance delalo jedno, a v nove pridalo button do ribbonu
		addFormToolbarButton: function(text, onclickFunc, url) {
			// přidá do toolbaru ve form nové tlačítko, po kliknutí zavolá funkci, nebo prejde na ulozene url
			var htmlId = text.replace(/ /g, "_");
			var href = "#";
			if (!onclickFunc && url) {
				href = url;
			}
			var htmlToAppend = '<td class=ms-separator>|</td>\n'
				   + '<td class="ms-toolbar" nowrap="true" id = "_spFocusHere">\n'
				   + "<table cellpadding='1' cellspacing='0'><tr><td class='ms-toolbar' nowrap>\n"
				   + "<a href='" + href + "' id='tbButton_" + htmlId + "' class='ms-toolbar'>" + text + "</a></td></tr></table></td>";

			$(".ms-toolbar .ms-toolbar[width='99%']").before(htmlToAppend);

			if (onclickFunc) {
				$("#tbButton_" + htmlId).bind("click", onclickFunc);
			}
		},

		// ve strance, kde je Form, a k nemu napojena webpart seznamu, prida button do toolbaru pripojene seznamove webpart
		addInlineViewToolbarButton: function(text, onclickFunc, url) {
			// přidá do toolbaru ve form nové tlačítko, po kliknutí zavolá funkci, nebo prejde na ulozene url
			var htmlId = text.replace(/ /g, "_");
			var href = "#";
			if (!onclickFunc && url) {
				href = url;
			}
			var htmlToAppend = '<td class=ms-separator>|</td>\n'
				   + '<td class="ms-toolbar" nowrap="true" id = "_spFocusHere">\n'
				   + "<table cellpadding='1' cellspacing='0'><tr><td class='ms-toolbar' nowrap>\n"
				   + "<a href='" + href + "' id='tbButton_" + htmlId + "' class='ms-toolbar'>" + text + "</a></td></tr></table></td>";

			$(".ms-menutoolbar .ms-toolbar[width='99%']").before(htmlToAppend);

			if (onclickFunc) {
				$("#tbButton_" + htmlId).bind("click", onclickFunc);
			}
		},

		verticalMultilookups: function(fields) {
			// převede multilookupy daného display name na dvouřádkové (širší zobrazení textu)
			var lookups = null;
			if (fields && fields.length > 0) {
				lookups = [];
				for (var i = 0; i < fields.length; i++) {
				    lookups.push($(NVR.e_n_d_form.getFormBody(fields[i])).find('.ms-long td:has(select)').parent());
				}
			}
			else {
				lookups = $('.ms-formbody .ms-long td:has(select)').parent();
			}

			$(lookups).each(function () {
				var tbl = $(this).parent().parent();
				var tds = tbl.find('td');

				if (tds.length == 5) {
					var row1 = $('<tr></tr>').appendTo(tbl);
					var row2 = $('<tr></tr>').appendTo(tbl);
					var row3 = $('<tr></tr>').appendTo(tbl);

					var leftcol = tds.eq(0);
					var buttons = tds.eq(2);
					var rightcol = tds.eq(4);

					row1.append(leftcol).find('div').width('100%').find('select').width('100%');
					row2.append(buttons).find('br').remove();
					row3.append(rightcol).find('div').width('100%').find('select').width('100%');

					tbl.find('tr').eq(0).remove();
				}
			});
		},

		_getNewLookupContents: function(internalName) {
			var newTd = null;
			$.ajax({
				cache: false,
				url: window.location.href,
				async: false,
				success: function (html) {
					newTd = $(html);
				}
			});
			return newTd;
		},

		_addNewLookupChoices: function(diffChoices, fieldIntName, confirmation) {
			if (diffChoices.length > 0) {
				var msg = NVR.general.hlaska("addNewLookupChoices"); //"Chcete použít nově vytvořené záznamy?";
				if (diffChoices.length == 1) {
				    msg = NVR.general.hlaska("addNewLookupChoice"); //"Chcete použít nově vytvořený záznam?";
				}

				if (confirmation && !confirm(msg + " (" + diffChoices.join("; ") + ")?")) {
					return;
				}

				for (var i = 0; i < diffChoices.length; i++) {
					putValueInInput(fieldIntName, diffChoices[i]);
				}
			}
		},

		_getLookupTds: function(fieldnames, webpart) {
			var tds = null;
			var fldtyperegexp = /FieldType="SPField(?!.*?Extended)(.*)Lookup"/m;
			if (!webpart) {
				webpart = $("table.ms-formtable");
			}

			if (fieldnames) {
				if (typeOf(fieldnames) == "string") {
					var fldnameregexp = new RegExp("(FieldName=\"" + fieldnames + "\")|(FieldInternalName=\"" + fieldnames + "\")", "m");
					tds = $(webpart).find("td.ms-formbody").filter(function () {
						var paramComment = $(this).contents()[0];

						if (!paramComment) return false;
						if (paramComment.nodeType != 8)
							paramComment = $(this).contents()[1];
						if (!paramComment || paramComment.nodeType != 8) return false;
						if (fldnameregexp.test(paramComment.nodeValue) && fldtyperegexp.test(paramComment.nodeValue)) return true;
						return false;
					});
				}
				else if (typeOf(fieldnames) == "array") {
					var query = [];
					for (var i = 0; i < fieldnames.length; i++) {
						query.push("(FieldName=\"" + fieldnames[i] + "\")|(FieldInternalName=\"" + fieldnames[i] + "\")");
					}

					// TODO
					var fldnameregexp = new RegExp(query.join("|"), "m");
					tds = $(webpart).find("td.ms-formbody").filter(function () {
						var paramComment = $(this).contents()[0];

						if (!paramComment) return false;
						if (paramComment.nodeType != 8)
							paramComment = $(this).contents()[1];
						if (!paramComment || paramComment.nodeType != 8) return false;
						if (fldnameregexp.test(paramComment.nodeValue) && fldtyperegexp.test(paramComment.nodeValue)) return true;
						return false;
					});
				}
			}
			else {
				tds = $(webpart).find("td.ms-formbody").filter(function () {
					var paramComment = $(this).contents()[0];
					if (!paramComment) return false;
					if (paramComment.nodeType != 8)
						paramComment = $(this).contents()[1];
					if (!paramComment || paramComment.nodeType != 8) return false;
					if (fldtyperegexp.test(paramComment.nodeValue)) return true;
					return false;
				});
			}
			return tds;
		},

		initMultiLookup: function(searchIn) {
			// funkce pro nastartování multilookupů po jejich načtení do stránky
			$(searchIn).find("input:hidden[id$=MultiLookupPicker]").each(function () {
				var masterId = $(this).attr("id").split("_MultiLookupPicker")[0];

				var candidateId = masterId + "_SelectCandidate";
				var resultId = masterId + "_SelectResult";
				var addButtonId = masterId + "_AddButton";
				var removeButtonId = masterId + "_RemoveButton";
				var dataId = masterId + "_MultiLookupPicker_data";
				var initial = masterId + "_MultiLookupPicker_initial";
				var currentSel = masterId + "_MultiLookupPicker";

				var master = null;
				eval("master = " + masterId + "_MultiLookupPicker_m");

				GipInitializeGroup(master, "" /* group control id */, candidateId, resultId, "" /* description control id */,
								   addButtonId, removeButtonId, dataId, initial, currentSel, 0,
								   true /* always allow delete */, true /* first */);
			});
		},

		openDialog: function(url, callback, options) {
			// options - http://www.chaholl.com/archive/2010/11/17/using-the-dialog-framework-in-sharepoint-2010.aspx
			// TODO - pridat dalsi parametr - funkce, ktera se zavola ve strance v otevrenem dialogu
			// je na to potreba pridat globalni pole dialogovych callbackovych funkci, a v runOnLoadedEverywhere
			// overit, ze jsme v modalnim okne, spustit callbackove funkce z window.top, a predat jim jako parametr $("body") nebo tak neco
			var opts = SP.UI.$create_DialogOptions();
			opts.url = url;
			if (callback) {
				opts.dialogReturnValueCallback = callback;
			}

			if (typeOf(options) == "object") {
				for (key in options) {
					opts[key] = options[key];
				}
			}

			SP.UI.ModalDialog.showModalDialog(opts);

			return false;
		},
		
		//prevadi data do podoby pro poslani pres POST
		PostConvert: function(retezec) {
			return NVR.Url.encode(retezec).replace(/\+/g, "%2b").replace(/%20/g, "+");
		},

		getCommonSaveFormRequestData: function() {
			return "MSOWebPartPage_PostbackSource=" +
			"&MSOTlPn_SelectedWpId=" + $("#MSOTlPn_SelectedWpId").val() +
			"&MSOTlPn_View=" + $("#MSOTlPn_View").val() +
			"&MSOTlPn_ShowSettings=" + $("#MSOTlPn_ShowSettings").val() +
			"&MSOGallery_SelectedLibrary=" + $("#MSOGallery_SelectedLibrary").val() +
			"&MSOGallery_FilterString=" + $("#MSOGallery_FilterString").val() +
			"&MSOTlPn_Button=" + $("#MSOTlPn_Button").val() +
			"&__EVENTTARGET=" + NVR.Url.encode($(':button[id$=SaveItem]').attr("name")) +
			"&__EVENTARGUMENT=" + $("#__EVENTARGUMENT").val() +
			"&__REQUESTDIGEST=" + PostConvert($("#__REQUESTDIGEST").val()) +
			"&MSOSPWebPartManager_DisplayModeName=" + $("#MSOSPWebPartManager_DisplayModeName").val() +
			"&MSOSPWebPartManager_ExitingDesignMode=" + $("#MSOSPWebPartManager_ExitingDesignMode").val() +
			"&MSOWebPartPage_Shared=" + $("#MSOWebPartPage_Shared").val() +
			"&MSOLayout_LayoutChanges=" + $("#MSOLayout_LayoutChanges").val() +
			"&MSOLayout_InDesignMode=" + $("#MSOLayout_InDesignMode").val() +
			"&_wpSelected=" + $("#_wpSelected").val() +
			"&_wzSelected=" + $("#_wzSelected").val() +
			"&MSOSPWebPartManager_OldDisplayModeName=" + $("#MSOSPWebPartManager_OldDisplayModeName").val() +
			"&MSOSPWebPartManager_StartWebPartEditingName=" + $("#MSOSPWebPartManager_StartWebPartEditingName").val() +
			"&MSOSPWebPartManager_EndWebPartEditing=" + $("#MSOSPWebPartManager_EndWebPartEditing").val() +
			"&__LASTFOCUS=" + $("#__LASTFOCUS").val() +
			"&_maintainWorkspaceScrollPosition=" + $("#_maintainWorkspaceScrollPosition").val() +
			"&__VIEWSTATE=" + NVR.Url.encode($("#__VIEWSTATE").val()).replace(/\//g, "%2F").replace(/\+/g, "%2B") +
			"&__SCROLLPOSITIONX=" + $("#__SCROLLPOSITIONX").val() +
			"&__SCROLLPOSITIONY=" + $("#__SCROLLPOSITIONY").val() +
			"&__EVENTVALIDATION=" + NVR.Url.encode($("#__EVENTVALIDATION").val()).replace(/\//g, "%2F").replace(/\+/g, "%2B") +
			"&SearchScope=" + NVR.Url.encode($('[name|="SearchScope"]').find('option').val()).replace(/\//g, "%2F") +
			"&SearchString=Search+this+site..." +
			"&" + NVR.Url.encode($(':input[id$=owshiddenversion]').attr("name")) + "=" + $(':input[id$=owshiddenversion]').val() +
			"&attachmentsToBeRemovedFromServer=&RectGifUrl=%2F_layouts%2Fimages%2Frect.gif&fileupload0=&__spText1=&__spText2=&_wpcmWpid=&wpcmVal=";
		},
		//pri updatovani formu se meni owshiddenversion proto ji musime potstrcit novou hodnotu
		securityExceptionHack: function (html) {
			var newBody = $(html);
			var newVersion = newBody.find('[name*="owshiddenversion"]').val();
			$('[name*="owshiddenversion"]').val(newVersion);

			$("#onetidinfoblockV").html(newBody.find("#onetidinfoblockV").html());
			$("#onetidinfoblock1").html(newBody.find("#onetidinfoblock1").html());
			$("#onetidinfoblock2").html(newBody.find("#onetidinfoblock2").html());
		},

		lookupValueAddingCallback: function(result, value) {
			try {
				//
				alert(NVR.general.hlaska("LookupValueAddingReload"));
				return;
				// NOVOU HODNOTU SE NEDARI ULOZIT, i kdyz se nacte a doplni - je jeste potreba poresit, jestli se z nove stranky spravne dotahuji validacni data
				var td = getFormBody(lookupValueAddingField); // globalni promenna lookupValueAddingField
				var fieldName = lookupValueAddingField;
				lookupValueAddingField = ""; // vynulovat pro priste
				var multiLookup = false;
				if ($(td).find("select").length > 1) {
					multiLookup = true;
				}
				var oldChoices;
				var newBody;
				var newTd;
				var newChoices;
				var diffChoices;
				if (multiLookup) {
					var masterId = $(td).find("input:hidden[id$=MultiLookupPicker]").attr("id").split("_MultiLookupPicker")[0];

					var candidateId = masterId + "_SelectCandidate";
					var resultId = masterId + "_SelectResult";
					var addButtonId = masterId + "_AddButton";
					var removeButtonId = masterId + "_RemoveButton";
					var dataId = masterId + "_MultiLookupPicker_data";
					var initial = masterId + "_MultiLookupPicker_initial";
					var currentSel = masterId + "_MultiLookupPicker";

					// zapamatovat si obsah levé části
					oldChoices = [];
					$("#" + candidateId).find("option").each(function () {
						oldChoices.push($(this).text());
					});

					// otevřeme nový NewForm a vezmeme si z něj čerstvý lookup, pak už do něj jen doplnit data
					newBody = _getNewLookupContents();
					newTd = getFormBody($(td).attr("FieldInternalName"), $(newBody));
					$(td).replaceWith(newTd);

					executeOnFullLoadedPageAsync(window.location.href, "lookupValueLoadIframe", function() {
						var newTd = getFormBody(fieldName, $('iframe#lookupValueLoadIframe').contents());

						var master = null;
						eval("master = " + masterId + "_MultiLookupPicker_m");
						$(td).replaceWith(newTd);
						var master = null;
						eval("master = " + masterId + "_MultiLookupPicker_m");

						GipInitializeGroup(master, "" /* group control id */, candidateId, resultId, "" /* description control id */,
									 addButtonId, removeButtonId, dataId, initial, currentSel, 0,
									 true /* always allow delete */, true /* first */);

						// porovnat novy obsah leve casti se starym, abychom ziskali hodnoty, ktere pribyly
						newChoices = [];
						$(newTd).find("option").each(function() {
							newChoices.push($(this).text());
						});
						diffChoices = $.richArray.diff(newChoices, oldChoices);
						_addNewLookupChoices(diffChoices, fieldName, true);
						$('iframe#lookupValueLoadIframe').remove();
						console.log("removed");
					});
				} else {
					oldChoices = [];
					$(td).find("option").each(function () {
						oldChoices.push($(this).text());
					});
					newBody = _getNewLookupContents();

					executeOnFullLoadedPageAsync(window.location.href, "lookupValueLoadIframe", function () {
						var newTd = getFormBody(fieldName, $('iframe#lookupValueLoadIframe').contents());

						$(td).replaceWith(newTd);

						// porovnat novy obsah leve casti se starym, abychom ziskali hodnoty, ktere pribyly
						newChoices = [];
						$(newTd).find("option").each(function () {
							newChoices.push($(this).text());
						});
						diffChoices = $.richArray.diff(newChoices, oldChoices);
						_addNewLookupChoices(diffChoices, fieldName, true);
						$('iframe#lookupValueLoadIframe').remove();
						console.log("removed");
					});

				}
				// dosadit nove security hodnoty
				// jinak vyzaduje ve web.configu vypnuty parametr enableEventValidation v sekci pages (<pages enableEventValidation="false" ... />)
				$("#__EVENTVALIDATION").val($(newBody).find("#__EVENTVALIDATION").val());
			}
			catch (err) {
				reportError(err);
			}
		},
		
		lookupValueAdding: function(fieldnames) {
			// funkce po spuštění přidá k vybraným lookupům ikonku, která po kliknutí dovolí vložit do lookupu nový řádek
			// fieldnames může být string nebo string array, budou to display names lookupových polí, které mají tuto
			// funkčnost podporovat
			// Id webu a seznamu bere ze stranky diky SPTools_Head.ascx v kazde strance
			if (!$.richArray) { // bude potreba pri volani callbacku, muze se nacist na pozadi
				$.getScript('/_layouts/15/SPTools/libs/jquery.rich-array.js');
			}
			var tds = null;
			if (fieldnames) {
				tds = this._getLookupTds(fieldnames);
			} else {
				tds = this._getLookupTds();
			}

			$(tds).each(function () {
				if ($(window).prev("td").find(".add2lookup").length != 0) return;

				var paramComment = $(window).contents()[0];

				if (paramComment.nodeType != 8)
				    paramComment = $(window).contents()[1];
				if (!paramComment || paramComment.nodeType != 8) return false;
				var fldname = paramComment.nodeValue;
				fldname = fldname.substring(fldname.indexOf('FieldInternalName="') + 19);
				fldname = fldname.substring(0, fldname.indexOf('"'));
				console.log("fldname ", fldname);

			    $(window)
					.prev("td")
					.find("h3 nobr")
					.prepend(" <a class='add2lookup' href='#'><img src='/_layouts/15/images/NEWITEM.GIF' border='0' width='16' height='16' alt='" + hlaska("addNewLookupAlt") + "' align='right' /></a>")
					.find("a").click(function () {
						lookupValueAddingField = fldname;
						console.log("fldname ", fldname);
						var lookupListId = WPQ2FormCtx.ListSchema[fldname].LookupListId;
						var newFormUrl = ContextGuids.siteurl + "/_layouts/15/listform.aspx?PageType=8"
									+ "&ListId=" + lookupListId
									+ "&WebId=" + ContextGuids.web
									+ "&Source=/_layouts/15/SPTools/CloseWindow.aspx";
						openDialog(newFormUrl, lookupValueAddingCallback);
						return false;
					});
			});
		},
		///////////////////////////////////////////////////////////////////// Form Backup // {{{
		// TODO rozpracovane kolegou, ktery byl odejit nez to dokoncil, v blize nezjistovanem stavu rozdelanosti - chtelo by to prozkoumat a dodelat
		// {{{
		_formBackup_GetContentFormHTML: function (loadActualContetn) { // {{{ NEFUNGUJE PRO FIREFOX - $(this).html() nevraci obsah s atributem VALUE, IE.8 jo

			var result = "";
			var saveData = {};

			$("table.ms-formtable td.ms-formbody[FieldType]").each(function () {
			    switch ($(window).attr("FieldType")) {
					case "SPFieldNote":
						// tváří se jako FieldNote, ale nezazálohoval by se dobře
					    if ($(window).attr("FieldInternalName") == "ConfigXML") { break; }
						// zkontrolovat, jestli obsahuje iframe, jinak rovnou pokracovat na default
					    var ifr = $(window).find("iframe[id$=_iframe]");
						if (ifr.length > 0) {
						    saveData[$(window).attr("FieldInternalName")] = $(ifr).contents().find("body").html();
							break;
						}
						saveData[$(window).attr("FieldInternalName")] = $(window).html();
						break;
					default:    // defaultne proste ulozime HTML
					    saveData[$(window).attr("FieldInternalName")] = $(window).html();
						break;
				}
			});

			//window.top.console.info(saveData["Title"]);
			var fbNewData = NVR.Base64.encode(JSON.stringify(saveData, function (key, value) { return value; }));


			var cmpOld = $.md5(fbOldData);
			var cmpNew = $.md5(fbNewData);

			//    window.top.console.info("fbNewData : "+cmpNew);
			//    window.top.console.info("fbOldData : "+cmpOld);

			if (fbOldData != fbNewData) {
				fbOldData = fbNewData;
				result = fbNewData;
			}
			if (loadActualContetn) {
				result = fbNewData;
			}

			window.top.console.info("result: " + result);
			return result;
		},


		_formBackupSave: function() {
			var saveString = this._formBackup_GetContentFormHTML(false);

			if (saveString == "")//nezmenil se obsah formu tak nic neukladame
			{
				return;
			}

			//------------------------------
			// return;
			//------------------------------

			// ulozit obsah iframes s editorem
			var saveUrl = "/_layouts/15/SPTools/FormBackup.aspx?ListUrl=" + NVR.Url.encode(getServerRelativeUrl(window.document.URL)) + "&ID=" + NVR.common.get_url_param("ID") + "&Save=TRUE";

			$.ajax({
				url: saveUrl,
				type: "POST",
				data: { backupData: saveString },
				success: function (xmlrequest, status) {
					addBackupToForms();
				}
			});
		},

		_formBackupWrite: function(backupData) {
			// přepsat
			var formrows = JSON.parse(backupData);

			$("table.ms-formtable td.ms-formbody[FieldType]").each(function () {
				switch ($(window).attr("FieldType")) {
					case "SPFieldNote":
					    var ifr = $(window).find("iframe[id$=_iframe]");
						if (ifr.length > 0) {
						    $(ifr).contents().find("body").html(formrows[$(window).attr("FieldInternalName")]);
							break;
						}
					default:    // defaultne proste ulozime HTML
						try {
						    $(window).html(formrows[$(window).attr("FieldInternalName")]);
						}
						catch (err) { NVR.general.reportError(NVR.general.hlaska("formBackupLoadProblem") + "<br>" + err); }
						break;
				}
			});

			// nastartovat lookupy
			this.initMultiLookup($(".ms-formtable"));
		},


		addBackupToForms: function() { // pridava select s verzemi backupů do prvniho radku new/edit formu
			$.ajaxSetup({ cache: false });
			//window.top.console.info("IN Funciotn: addBackupToForms");

			var LoadUrl = "/_layouts/15/SPTools/FormBackup.aspx?ListUrl=" + NVR.Url.encode(getServerRelativeUrl(window.document.URL)) + "&ID=" + NVR.common.get_url_param("ID") + "&Save=FALSE";

			$.ajax(
			{
				dataType: "json",
				url: LoadUrl,
				type: "POST",
				cache: false,
				success: function (data) {
					backup_data = data;
					if (NVR.originalBackup != "") {
						backup_data["Stav před obnovou"] = NVR.originalBackup;
					}

					$("#formBackup").remove();

					/*var placeToLoad = $("table.ms-formtable"); // PRVNI RADEK FORMU
					placeToLoad.prepend("<tr><td nowrap='true' valign='top' width='190px' class='ms-formlabel'><h3 class='ms-standardheader'><nobr>Backup Data</nobr></h3></td>" +
					"<td valign='top' class='ms-formbody'><span dir='none'><select id='formBackup'></select></span></td></tr>"); */

					var placeToLoad;
				    var vyska;
					if ($("table.ms-formtoolbar").eq(1).find("td.ms-toolbar").length == 3) //Neverzovanej form
					{
						placeToLoad = $("table.ms-formtoolbar").eq(1).find("td.ms-toolbar").eq(0).attr("align", "right");
						vyska = $("table.ms-formtoolbar").eq(1).find("td.ms-toolbar").eq(1).height();
					}
					else {
						placeToLoad = $("table.ms-formtoolbar").eq(1).find("td.ms-toolbar").eq(1).attr("align", "right");
						vyska = $("table.ms-formtoolbar").eq(1).find("td.ms-toolbar").eq(2).find("table").height();
					}

					placeToLoad.css({ "padding-right": "5px" });
					placeToLoad.prepend("<select id='formBackup'></select>");

					vyska = vyska - 1;

					var select = $("#formBackup");
					$(select).css({ "height": vyska }); // pro FF "position" : "relative", "top" : "-4px"});
					$(select).find("option").remove();
					$(select).bind("change", function (e) {
						// upozornit, že to celý přepíšem
						if (this.options[this.selectedIndex].value != "" && confirm("Přepsat obsah formuláře zálohou '" + this.options[this.selectedIndex].text + "'?")) {
							//$.blockUI();
							// zazálohovat aktuální ­ stav a umístit ho jako první option
						    if (NVR.originalBackup == "") {
						        NVR.originalBackup = _formBackup_GetContentFormHTML(true);
						        backup_data["Stav před obnovou"] = NVR.originalBackup;

								//PREPEND NELZE POUZIT pri rozbaleni selectu ta polozka neni videt,ale pri vyberu sipkama tam je
								//$(select).prepend("<option value='AAAA'>AAAAA</option>")

								this.options[0].value = "Stav před obnovou";
								this.options[0].text = "Stav před obnovou";
							}

							_formBackupWrite(NVR.Base64.decode(backup_data[this.options[this.selectedIndex].value]));
							$.unblockUI();
						}
					});

					// window.top.console.info("originalBackup: "+originalBackup);
					if (NVR.originalBackup != "") {
						$(select).append("<option value='Stav před obnovou'>Stav před obnovou</option>");
					}
					else {
						for (var i in backup_data)  //pridame zalohy do selectu
						{
							if (i != "Stav před obnovou") {
								$(select).append("<option value='null'>Current:" + i + "</option>");
								break;
							}
						}
					}

					for (i in backup_data)  //pridame zalohy do selectu
					{
						if (i != "Stav před obnovou") {
							var pole = i.split('#');
							var datum = new Date(pole[1]);
							datum = datum.addHours(1);

							//$(select).append("<option value='" + i + "'>" + datum.format("dd/MM/yyyy h:mm:ss tt") + "</option>");
							$(select).append("<option value='" + i + "'>" + i + "</option>");
							//window.top.console.info("data[i] - "+i);
						}
					}
				}
			});
		},

		enableJSTree: function() { // TODO - stromova prace s knihovnou dokumentu, zatim neni ani nacato
			// dotáhne do stránky tree knihovny
			NVR.general.headAppend("<script type='text/javascript' src='/_layouts/15/libs/jquery/jquery.tree.js'></script>");
			NVR.general.headAppend("<script type='text/javascript' src='/_layouts/15/libs/jquery/jquery.history_remote.js'></script>");
		},

		AddTree: function(element, url) {
			if (!$.tree) {
				this.enableJSTree();
			}
			$(element).tree({
				data:
					{
						type: "json",
						opts: {
							url: "/_layouts/15/SPTools/DirectoryTree.aspx?Url=" + url,
							method: "GET"
						}
					},
				callback:
					{
						onselect: function (node, treeobj) {
							var folder = null;
							if ($(node).attr("folder")) {
								folder = NVR.Url.encode($(node).attr("folder"));
							}
							reloadListView($(node).parents().find("#dirtree_details"),
															$(node).attr("url"),
															$(node).attr("web"),
															folder);

							return false;
						}
					}
			});

		}
	
	};
	
	return NVR;
});