define(['jquery', 'underscore', 'nvr', 'nvr_notifications'/*,...*/], function ($, _, NVR/*,...*/) {
	"use strict";
	
	NVR.general = {
		pageLoad: function(sender, args) {	//ponechan jenom runOnEachPostback
			ExecuteOrDelayUntilScriptLoaded(function () {
				NVR.runOnEachPostback.reverse();
				_.each(NVR.runOnEachPostback, function (runItem) {
					runItem(sender, args);
				});
			}, "core.js");//"sp.core.js se nenacita vsude core.js by snad mohl..."
		},
		stacktrace: function(){
			function st2(f) {
				return !f ? [] :
					st2(f.caller).concat([f.toString().split('(')[0].substring(9) + '(' + JSON.stringify(f.arguments) + ')']);
			}

			return st2(arguments.callee.caller);
		},
		// http://www.eriwen.com/javascript/js-stack-trace/
		printStackTrace: function() {
			var callstack = [];
			var isCallstackPopulated = false;
			try {
				i.dont.exist += 0; //doesn't exist- that's the point
			} catch (e) {
				if (e.stack) { //Firefox
					var lines = e.stack.split('\n');
					for (var i = 0, len = lines.length; i < len; i++) {
						if (lines[i].match(/^\s*[A-Za-z0-9\-_\$]+\(/)) {
							callstack.push(lines[i]);
						}
					}
					//Remove call to printStackTrace()
					callstack.shift();
					isCallstackPopulated = true;
				} else if (window.opera && e.message) { //Opera
					var lines = e.message.split('\n');
					for (var i = 0, len = lines.length; i < len; i++) {
						if (lines[i].match(/^\s*[A-Za-z0-9\-_\$]+\(/)) {
							var entry = lines[i];
							//Append next line also since it has the file info
							if (lines[i + 1]) {
								entry += ' at ' + lines[i + 1];
								i++;
							}
							callstack.push(entry);
						}
					}
					//Remove call to printStackTrace()
					callstack.shift();
					isCallstackPopulated = true;
				}
			}
			if (!isCallstackPopulated) { //IE and Safari
				var currentFunction = arguments.callee.caller;
				while (currentFunction) {
					var fn = currentFunction.toString();
					var fname = fn.substring(fn.indexOf("function") + 8, fn.indexOf('')) || 'anonymous';
					callstack.push(fname);
					currentFunction = currentFunction.caller;
				}
			}
			return callstack.join("\n\n");
		},
		reportWarningRemote: function(msg, src) {
			var st = "";
			src = typeof src !== 'undefined' ? src : "NVR";
			try {
			    st = NVR.general.printStackTrace();
			} catch (ee) {
				st = "exception getting stack trace - " + ee.toString;
			};
			$.get("/_layouts/15/SPTools/JSErrorReport.aspx", { "Source": src, "Message": msg + "\n\nStack trace:\n" + st, "Type": "warning" });
		},
		reportErrorRemote: function(msg, src) {
			var st = "";
			src = typeof src !== 'undefined' ? src : "NVR";
			try {
			    st = NVR.general.printStackTrace();
			} catch (ee) {
				st = "exception getting stack trace - " + ee.toString;
			};
			$.get("/_layouts/15/SPTools/JSErrorReport.aspx", { "Source": src, "Message": msg + "\n\nStack trace:\n" + st, "Type": "error" });
		},
		executeOnFullLoadedPageAsync: function (urlToLoad, iframeId, funcToExec) {
			// ie nenacte obsah skryteho iframe, a ani zobrazeny iframe nenacte, dokud ma jeho url v sobe parametr Source (wtf?!)
			var processedUrl = urlToLoad.substring(urlToLoad.indexOf('?') + 1);
			if (urlToLoad.indexOf('?') != -1) {
				processedUrl = urlToLoad.substring(0, urlToLoad.indexOf('?') + 1) + processedUrl.replace(/&Source(\=[^&]*)?(?=&|$)|^Source(\=[^&]*)?(&|$)/, "");
			}

			var ifr = $("<iframe width='1' height='1' id='" + iframeId + "' src='" + processedUrl + "' />");

			$("body").append(ifr);

			$(ifr).load(function () {
				var w = $(ifr).get(0).contentWindow;
				/*var delayedCall = function() {
					if (!w.ExecuteOrDelayUntilScriptLoaded) {
						setTimout(delayedCall, 500);
						console.log("delayed call");
					} else {*/
				//console.log("ExecuteOrDelayUntilScriptLoaded");
				w.ExecuteOrDelayUntilScriptLoaded(funcToExec, "core.js");
				/*}
			};
			delayedCall();*/
			});
			return $(ifr);
		},
		getLookupValue: function(lookuptext) {
			if (lookuptext == undefined) return undefined;
			if (lookuptext.indexOf(";#") == -1) return lookuptext;
			return lookuptext.substring(lookuptext.indexOf("#") + 1, lookuptext.length);
		},
		getUserLogin: function(login) {
			var s = login;
			var bsIndex = s.indexOf("\\");
			if (bsIndex != -1) {
				s = s.substring(bsIndex + 1);
			}
			return s;
		},
		hlaska: function(title, defaultMsg, context) {
			//return; // TODO
			var result;
			if (!window.Hlasky) {
				if (window.top.console) {
					window.top.console.log("Chybi globalni Hlasky");
				}
				if (defaultMsg) {
					result = defaultMsg;
				} else {
					result = title;
				}
			} else if (Hlasky[title]) {
				result = Hlasky[title];
			} else {
				try {
					result = eval("Hlasky." + title);
				} catch (e) { };

				if (!result && window.top.console) {
					window.top.console.log("Chybejici hlaska: %s", title);
					if (defaultMsg) {
						result = defaultMsg;
					} else {
						result = title;
					}
				}
			}
			if (context) {
				return _.template(result, context);
			} else {
				return result;
			}
		},
		reportError: function(msg, showstatus) {
			if (window.top.console) {
				if (window.top.console.error) {
					window.top.console.error(msg);
				} else if (window.top.console.log) {
					window.top.console.log(msg);
				}
			}
			if (showstatus === undefined || !showstatus) {
			    NVR.notifications.showStatus(NVR.general.hlaska("Error"), msg, "red", true, null, "_reportError");
				reportErrorRemote(msg);
			}
		},
		reportStatus: function(msg, sticky) {
		    NVR.notifications.showStatus(NVR.general.hlaska("Message"), msg, "blue", !(sticky === undefined || !sticky), null, "_reportStatus");
		},
		getActiveDialogFrame: function() {
			// ziskat aktivni dialogovat okno - tedy to, ktere ma nejvyssi z-index, zatim nevim o lepsim zpusobu
			var activeFrame = null;
			var maxIndex = 0;
			$(".ms-dlgFrame").each(function () {
				var index = $(this).parents(".ms-dlgContent").css("z-index");
				if (index > maxIndex) {
					activeFrame = this;
					maxIndex = index;
				}
			});
			return activeFrame;
		},
		// je stranka DispForm, NewForm nebo EditForm?  depends on ContextGuids
		isFormPage: function() {
			return window.ContextGuids.isForm == "true";
		},
		isNewFormPage: function() {
			return window.ContextGuids.isNewForm == "true";
		},
		isEditFormPage: function() {
			return window.ContextGuids.isEditForm == "true";
		},
		isDisplayFormPage: function() {
			return window.ContextGuids.isDisplayForm == "true";
		},
		isListViewPage: function() {
			// vraci true jestlize je stranka view seznamu
			return window.ContextGuids.isViewForm == "true";
			// return ($("table.ms-listviewtable, table.ms-emptyView").length > 0);
		},
		// prida do stranky CSS style
		headAppendCSS: function(fileName){
			var $fileref = $("<link />");
			$fileref.attr("rel", "stylesheet");
			$fileref.attr("type", "text/css");
			$fileref.attr("href", fileName);
			document.getElementsByTagName("head")[0].appendChild($fileref[0]);
		},
		// prida kus kodu do hlavicky stranky
		headAppend: function(piece) {
			$("head").append(piece);
		},
		// dotáhne do stránky knihovny pro jqGrid
		enableJQGrid: function() {
		    if ($.jqGrid) return;
		    //$.getScript || require();
			//loadScript('/_layouts/15/SPTools/libs/jquery.jqGrid.js');
			headAppendCSS('/_layouts/15/SPTools/libs/ui.jqgrid.css');
		},
		enableMultiSelect: function () {
		    //$.getScript || require();
			//loadScript('/_layouts/15/SPTools/libs/jquery.multiselect.min.js');
			headAppendCSS('/_layouts/15/SPTools/libs/jquery.multiselect.css');
		},
		enableJQueryUI: function (themeDir) { // partly depends on ContextGuids
			if ($.widget) return;

			if (!themeDir) {
				themeDir = "smoothness";
			}

		    //$.getScript || require();
			//loadScript('/_layouts/15/SPTools/libs/jquery-ui.js');
			//loadScript('/_layouts/15/SPTools/libs/localization/jquery.ui.datepicker-' + ContextGuids.userlangiso + '.js');
			//loadScript('/_layouts/15/SPTools/libs/localization/jquery.ui.datepicker-' + ContextGuids.userlang + '.js');

			headAppendCSS('/_layouts/15/SPTools/libs/uithemes/' + themeDir + '/jquery-ui.css');
			if ($.datepicker.regional[ContextGuids.userlangiso])
				$.datepicker.setDefaults($.datepicker.regional[ContextGuids.userlangiso]);
			else if ($.datepicker.regional[ContextGuids.userlang])
				$.datepicker.setDefaults($.datepicker.regional[ContextGuids.userlang]);
		},
		
		remapGetSource: function(url) { 
			//window.top.console.log("remapping GetSource");
			// premapuje GetSource na aktualni stranku, pripadne na dodane url
			var curloc = Url.encode(window.location.href);

			if (url) {
				curloc = url;
			}

			window.GetSource = function () {
				//window.top.console.log("using remapped GetSource");
				return curloc;
			};
		},
		Custom_AddListMenuItems: function(m, ctx) {
			_.each(NVR.globalContextMenuItems, function (menuItem) {
				if (menuItem["sepBefore"])
					CAMSep(m);

				CAMOpt(m, menuItem["displayText"], menuItem["action"], menuItem["imagePath"]);

				if (menuItem["sepAfter"])
					CAMSep(m);
			});

			return false;
		},
		Custom_AddSendSubMenu: function(m, ctx) {
			if (typeof L_Send_Tex == 'undefined') return; //SHP2013 resolve bug

			ULSrLq:;
			var strDisplayText = L_Send_Text;
			var currentItemUrl = GetAttributeFromItemTable(itemTable, "Url", "ServerUrl");
			var currentItemEscapedFileUrl = "";
			var strImagePath = "";
			var currentItenUnescapedUrl;
			var strExtension;
			var strAction;

			if (currentItemFileUrl != null) {
				currentItenUnescapedUrl = unescapeProperly(currentItemFileUrl);
				currentItemEscapedFileUrl = escapeProperly(currentItenUnescapedUrl);
				strExtension = SzExtension(currentItenUnescapedUrl);
				if (strExtension != null && strExtension != "")
					strExtension = strExtension.toLowerCase();
			}

			var serverFileRedirect = itemTable.getAttribute("SRed");
			var iDefaultIO = itemTable.getAttribute("DefaultIO");
			if (iDefaultIO == "0" && !HasRights(0x0, 0x20))
				iDefaultIO = "1";
			var otherLocationMenuItemCondition = (currentItemProgId != "SharePoint.WebPartPage.Document") &&
					(serverFileRedirect == null || serverFileRedirect == "" || HasRights(0x0, 0x20)) && (strExtension != "aspx");
			var sendToEmailMenuAttachmentItemCondition = HasRights(0x10, 0x0);
			var sendToEmailMenuItemCondition = HasRights(0x10, 0x0);
			var downloadACopyMenuItemCondition = (currentItemFSObjType != 1) &&
					(ctx.listBaseType == 1) && (serverFileRedirect == null || serverFileRedirect == "" || HasRights(0x0, 0x20));

			if ((!otherLocationMenuItemCondition) && (!sendToEmailMenuAttachmentItemCondition) && (!sendToEmailMenuItemCondition) && (!downloadACopyMenuItemCondition)) return;

			var sm = CASubM(m, strDisplayText, "", "", 400);
			CUIInfo(sm, "SendTo", ["SendTo", "PopulateSendToMenu"]);
			sm.IsSubMenu = true;
			sm.id = "ID_Send";
			var menuOption;
			if (otherLocationMenuItemCondition) {
				if (typeof (ctx.SendToLocationName) != "undefined" &&
					ctx.SendToLocationName != null &&
						ctx.SendToLocationName != "" &&
							typeof (ctx.SendToLocationUrl) != "undefined" &&
								ctx.SendToLocationUrl != null &&
									ctx.SendToLocationUrl != "") {
					strAction = "STSNavigate('" + ctx.HttpRoot + "/_layouts/15/copy.aspx?" + "SourceUrl=" + currentItemEscapedFileUrl + "&FldUrl=" + escapeProperly(ctx.SendToLocationUrl);
					strAction = AddSourceToUrl(strAction) + "')";
					menuOption = CAMOpt(sm,
						ctx.SendToLocationName,
						strAction,
						"");
					CUIInfo(menuOption, "SendToRecommendedLocation", ["SendToRecommendedLocation"]);
				}
				if (typeof (itemTable.getAttribute("HCD")) != "undefined" && itemTable.getAttribute("HCD") == "1") {
					strDisplayText = L_ExistingCopies_Text;
					strAction = "STSNavigate('" + ctx.HttpRoot + "/_layouts/15/updatecopies.aspx?" + "SourceUrl=" + currentItemEscapedFileUrl;
					strAction = AddSourceToUrl(strAction) + "')";
					strImagePath = ctx.imagesPath + "existingLocations.gif";
					menuOption = CAMOpt(sm, strDisplayText, strAction, strImagePath);
					menuOption.id = "ID_ExistingCopies";
					CUIInfo(menuOption, "SendToExistingCopies", ["SendToExistingCopies"]);
				}
				strDisplayText = L_OtherLocation_Text;
				strAction = "NavigateToSendToOtherLocationV4(event, '" + ctx.HttpRoot + "/_layouts/15/copy.aspx?" + "SourceUrl=" + currentItemEscapedFileUrl;
				strAction = AddSourceToUrl(strAction) + "')";
				strImagePath = ctx.imagesPath + "sendOtherLoc.gif";
				menuOption = CAMOpt(sm, strDisplayText, strAction, strImagePath);
				menuOption.id = "ID_OtherLocation";
				CUIInfo(menuOption, "SendToOtherLocation", ["SendToOtherLocation"]);
				if (ctx.OfficialFileNames != null && ctx.OfficialFileNames != "") {
					var ar_officialFileNames = ctx.OfficialFileNames.split("__HOSTSEPARATOR__");
					if (ar_officialFileNames != null) {
						for (var count = 0; count < ar_officialFileNames.length; count++) {
							var strSerializedText = ar_officialFileNames[count];
							var ar_OfficialFileHost = strSerializedText.split(",");
							strDisplayText = ar_OfficialFileHost[0];
							var index = 0;
							var action = "Copy";
							if (ar_OfficialFileHost.length == 3) {
								strDisplayText = ar_OfficialFileHost[0].replace(/%2c/g, ",").replace(/%25/g, "%");
								index = ar_OfficialFileHost[1];
								action = ar_OfficialFileHost[2];
							}
							strAction = "if(confirm(\"" + StBuildParam(SubmitFileConfirmation[action], STSScriptEncode(strDisplayText)) + "\")!=0) SubmitFormPost('" + ctx.HttpRoot + "/_layouts/15/SendToOfficialFile.aspx?" + "ID=" + escapeProperly(strDisplayText) + "&Index=" + index + "&SourceUrl=" + currentItemEscapedFileUrl;
							strAction = AddSourceToUrl(strAction) + "')";
							strImagePath = "";
							menuOption = CAMOpt(sm, strDisplayText, strAction, strImagePath);
							var strRibbonCmd = "SendToOfficialFile" + count;
							CUIInfo(menuOption, strRibbonCmd, [strRibbonCmd]);
						}
					}
				}
				CAMSep(sm);
			}

			if (sendToEmailMenuAttachmentItemCondition)//atachment
			{
			    strDisplayText = NVR.general.hlaska("SPTools.EmailAttachment");
				currentItemUrl = GetAttributeFromItemTable(itemTable, "Url", "ServerUrl");
				var httpRootWithSlash = ctx.HttpRoot.substr(0);
				if (httpRootWithSlash[httpRootWithSlash.length - 1] != '/')
					httpRootWithSlash += '/';
				var slashLoc = -1;
				var fileUrl = "";
				slashLoc = httpRootWithSlash.substring(8).indexOf('/') + 8;
				fileUrl = httpRootWithSlash.substr(0, slashLoc) + escapeProperlyCore(unescapeProperly(currentItemUrl), true);
				strAction = "javascript:SendSingleDoc(ContextGuids.list, currentItemID)";
				strImagePath = ctx.imagesPath + "gmailnew.gif";
				menuOption = CAMOpt(sm, strDisplayText, strAction, strImagePath);
				CUIInfo(menuOption, "EmailAttachment", ["EmailAttachment"]);
				menuOption.id = "ID_SendToEmailAttachment";
			}

			if (sendToEmailMenuItemCondition) {
				strDisplayText = L_SendToEmail_Text;
				currentItemUrl = GetAttributeFromItemTable(itemTable, "Url", "ServerUrl");
				var httpRootWithSlash = ctx.HttpRoot.substr(0);
				if (httpRootWithSlash[httpRootWithSlash.length - 1] != '/')
					httpRootWithSlash += '/';
				var slashLoc = -1;
				var fileUrl = "";
				slashLoc = httpRootWithSlash.substring(8).indexOf('/') + 8;
				fileUrl = httpRootWithSlash.substr(0, slashLoc) + escapeProperlyCore(unescapeProperly(currentItemUrl), true);
				strAction = "javascript:SendEmail('" + fileUrl + "')";
				strImagePath = ctx.imagesPath + "gmailnew.gif";
				menuOption = CAMOpt(sm, strDisplayText, strAction, strImagePath);
				CUIInfo(menuOption, "EmailLink", ["EmailLink"]);
				menuOption.id = "ID_SendToEmail";
			}
			if (downloadACopyMenuItemCondition) {
				if (ctx.listTemplate != 109 &&
					ctx.listTemplate != 119)
					AddWorkspaceMenuItem(sm, ctx);
				if (ctx.listTemplate != 119) {
					strAction = "STSNavigate('" + ctx.HttpRoot + "/_layouts/15/download.aspx?" + "SourceUrl=" + currentItemEscapedFileUrl + "&FldUrl=" + escapeProperly(ctx.SendToLocationUrl);
					strAction = AddSourceToUrl(strAction) + "')";
					menuOption = CAMOpt(sm, L_DownloadACopy_Text, strAction, "");
					CUIInfo(menuOption, "DownloadCopy", ["DownloadCopy"]);
					menuOption.id = "ID_DownloadACopy";
				}
			}
		},
		appendToUrl: function(url, params) {
			// pridava parametry za existujici url - pokud zadne nejsou, doda otaznik, pokud ano, prida ampersandove
			// pokud parametr vyzaduje encode, pozna to a zakoduje ho
			// muze dostat budto rovnou string (x=y&z=w) nebo slovnik

			function encodeIfNeeded(part) {
				// pokud part potrebuje Url.encode, vrati ho encodovany
				// TODO test by se dal urcite rozsirit
				if (part.indexOf("&") != -1 || part.indexOf("?") != -1) {
					return Url.encode(part);
				}
				return part;
			}

			var newUrl = url;
			var newParams = "";

			var params2 = [];
			if (params && typeOf(params) == "string") {
				params2 = params;

				// jestli uz params string zacina spojovacim znakem, zrusime ho, protoze ho potrebujeme pridat pozdeji a po svem
				if (params2.startsWith("?") || params2.startsWith("&")) {
					params2 = encodeIfNeeded(params2.substring(1));
				}

				newParams = params2;
			} else if (params && typeOf(params) == "object") {
				_.each(params, function (val, key) {
					var strKey = encodeIfNeeded(key.toString());
					var strVal = "";
					if (val) { // hodnota by mohla byt null
						strVal = encodeIfNeeded(val.toString());
					}
					params2.push(strKey + "=" + strVal);
				});
				newParams = params2.join("&");
			}

			// pokud url uz nejake parametry obsahuje, budeme pripojovat pres ampersandy, jinak pres otaznik
			if (newUrl.indexOf("?") == -1) {
				newUrl += "?" + newParams;
			} else {
				newUrl += "&" + newParams;
			}

			return newUrl;
		}
	};
	
	return NVR;
});