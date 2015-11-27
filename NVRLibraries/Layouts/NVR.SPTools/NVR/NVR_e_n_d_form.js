define(['jquery', 'underscore', 'nvr', 'nvr_base64_url', 'nvr_general', 'nvr_common'/*,...*/], function ($, _, NVR/*,...*/) {
	"use strict";
	
	NVR.e_n_d_form = {
		allFormRows: function(webpart) {
			var result = {};
			if (!webpart) {
				webpart = $("table.ms-formtable");
			}
			$("table.ms-formtable").find("tr[id!='idAttachmentsRow']").each(function () {
				var keyname = $.trim($(window).find("td.ms-formlabel").text());
				if (keyname.endsWith(" *")) {
					keyname = keyname.substring(0, keyname.length - 2);
				}

				var value = this.getFormBodyText(keyname, webpart);
				result[keyname] = value;
			});

			return result;
		},

		// pripravi cookie pro prefill nějakého formu
		setPrefillCookie: function(values, additional_id) {
			var valueArray = [];

			_(values).each(function (value, key) { valueArray.push(key + "=" + value); });

			var cookieValue = valueArray.join("&");

			var nowPlus3Min = new Date();
			nowPlus3Min = new Date(nowPlus3Min.setMinutes(nowPlus3Min.getMinutes() + 3));

			setCookie("prefillForm" + additional_id, cookieValue, nowPlus3Min, "/");
			window.top.console.info("setting prefill cookie: prefillForm%s - '%s'", additional_id, cookieValue);
			return cookieValue;
		},

		autoConnectPrefill: function(originalDict, hideRow, hideCol) { //? jqListHeaders
			var dict = {};
			if (originalDict && typeOf(originalDict) == "object") {
				dict = originalDict;
			}

			// podivat se do sloupcu na nastaveni filtrovani
			var sortfields = jqListHeaders().find("table[sortfields!='']").attr("sortfields");
			if (!sortfields) {
				sortfields = jqListHeaders().find("div[sortfields!='']").attr("sortfields");
			}
			var sortSettings = get_url_params(sortfields);
			var foundFilters = {};
			_(sortSettings).each(function (value, key) {
				if (key.startsWith("FilterField")) {
					var index = key.substring(11);
					var val = $.trim(nbspReplace(NVR.Url.decode(sortSettings["FilterValue" + index])));
					if (val.indexOf(";#") != -1) {
					    val = NVR.general.getLookupValue(val);
					}
					foundFilters[NVR.Url.decode(sortSettings[key])] = val;
				}
			});

			// prohlednout form, jestli se tam nachazi hodnota nektereho filtru
			var rows = this.allFormRows();
			_(foundFilters).each(function (filter, key) {
				_(rows).each(function (fieldName, fKey) {
					// hodnota ve vyfiltrovanem sloupci je hodnota jednoho z radku formu, nastavime automaticky prefill a sloupec i radek/radky schovame
					if (fieldName == $.trim(filter)) {
						dict[key] = fieldName;
						if (typeOf(hideCol) == "undefined" || hideCol) { // defaultne schovavame
							hideColumn(key);
						}
						if (typeOf(hideRow) == "undefined" || hideRow) { // defaultne schovavame
							hideFormRow(fKey);
						}
					}
				});
			});

			return dict;
		},

		autoPrefillForm: function () { // depends on ContextGuids, cookies - replace with html5 storage?

			var cookieName;
			if (ContextGuids.list != undefined) {
				cookieName = "prefillForm" + ContextGuids.list;
			}
			else {
			    cookieName = "prefillFormnewformcontenttype";
			}

			var prefillCookie = getCookie(cookieName);
			window.top.console.info("loading prefillCookie %s - '%s'", cookieName, prefillCookie);

			if (prefillCookie) {
				var data = get_url_params(prefillCookie);
				var hidePrefilled = [];
				var actuallyPrefilled = [];
				_(data).each(function (item, key) {
					if (key == "_HidePrefilled") {
						var spl = item.split(";");
						for (var i = 0; i < spl.length; i++) {
							hidePrefilled.push(spl[i]);
						}
						return;
					}

					if (key == "_HideAlways") {
						var spl = item.split(";");
						for (var i = 0; i < spl.length; i++) {
							hideFormRow(spl[i]);
						}
						return;
					}

					var resultingStr = item;
					if (resultingStr.indexOf("%20") != -1) {
						resultingStr = unescape(resultingStr);
					}
					putValueInInput(key, $.trim(resultingStr), false);
					actuallyPrefilled.push(key);
				});

				// pokud je pole v actuallyPrefilled, tedy bylo doopravdy prefillnuto, schovame jeho radek
				_.each(
					_(hidePrefilled).select(function (hide) { return _(actuallyPrefilled).indexOf(hidePrefilled[hide]) == -1; }),
					function (val) {
						hideFormRow(val);
					});

				// vymazat cookie - prazdnej obsah pouze cookie odstrani, null vymaze
				setCookie(cookieName, null, new Date(), "/");
			}
		},

		// premapuje funkci NewItem, takze se jeste pred ni ulozi prefillCookie
		// pokud je parametr autoConnect true, pokusi se najit lookup, kterym je napojeny seznam na form a prefillovat ho automaticky
		newItemPrefill: function(dict, autoConnect, hideFormRow, hideListColumn) {
			var NewNewItem2 = NewItem2;
			NVR.prefillDict = dict; // prefillDict je globalni promenna, abychom se k tomu dostali odkudkoliv

			if (autoConnect) {
				NVR.prefillDict = this.autoConnectPrefill(NVR.prefillDict, hideFormRow, hideListColumn);
			}

			NewItem2 = function (evt, url) {
				var listGuid = NVR.Url.decode(NVR.common.get_url_param("ListId", url).toLowerCase());

				if (listGuid.startsWith("{")) {
					listGuid = listGuid.substring(1);
				}
				if (listGuid.endsWith("}")) {
					listGuid = listGuid.substring(0, listGuid.length - 1);
				}

				this.setPrefillCookie(NVR.prefillDict, listGuid);
				return NewNewItem2(evt, url);
			};
		},


		hasCell: function(cellName) {
			// má formulář řádek s tímto display name?
			if (!__formCells) {
				__formCells = $("h3.ms-standardheader nobr");
			}

			for (var i = 0; i < __formCells.length; i++) {
				if (__formCells[i].firstChild.textContent) {
					if (__formCells[i].firstChild.textContent == cellName) {
						return true;
					}
				}
				else {
					if (__formCells[i].firstChild.nodeValue == cellName) {
						return true;
					}
				}
			}

			return false;
		},

		hideFormRow: function(name) {
			// skryje řádek formu jménem name
			this.getFormBody(name).parent().hide();
		},

		appendHrefSource: function(cellName, webpart, location) {
			// pro display name řádku vrací jquery objekt s obsahem editačního pole
			var loc = window.location;

			if (!webpart) {
				webpart = $("table.ms-formtable");
			}

			if (location) {
				loc = location;
			}

			var target = webpart;

			if (cellName) {
				target = this.getFormBody(cellName, webpart);
			}

			target.find("a")
				.each(function () {
					var link = NVR.general.appendToUrl(url, "Source=" + loc);
					$(this).attr("href", link);
				});
		},

		getFormBody: function(cellName, webpart) {
			// pro display name řádku vrací jquery objekt s obsahem editačního pole
			if (!webpart) {
				// musi pocitat s tim, ze pokud je aktivni dialogove okno, vztahuje se to k nemu
				var fr = NVR.general.getActiveDialogFrame();
				if (fr) {
					if (fr[0] && fr[0].contentWindow) {
						webpart = $(fr[0].contentWindow.document).find("table.ms-formtable");
					} else if (fr.contentWindow) {
						webpart = $(fr.contentWindow.document).find("table.ms-formtable");
					}

				}
				else {
					webpart = $("table.ms-formtable");
				}
			}

			var elem = $(webpart).find("td.ms-formbody").filter(function () {
				var paramComment = $(this).contents()[0];
				if (!paramComment) return false;
				if (paramComment.nodeType != 8)
					paramComment = $(this).contents()[1];
				if (!paramComment || paramComment.nodeType != 8) return false;
				if (paramComment.nodeValue.indexOf('FieldInternalName="' + cellName + '"') != -1
					|| paramComment.nodeValue.indexOf('FieldName="' + cellName + '"') != -1) return true;
				return false;
			});

			return elem;
		},

		getFormBodyText: function(cellName, webpart) {
			// vrací text, který je v editační části řádku s display name cellname
			// u editformu se musí dívat do inputů apod.
			// TODO zkontrolovat, že funguje i s rich text area a vůbec se vším
			var cell = this.getFormBody(cellName, webpart);
			var txt = "";
			var inp = cell.find("input, textarea, select");
			if (inp.length > 0) {
				txt = inp.val();
			} else {
				txt = cell.text();
			}

			return $.trim(txt);
		},

		getFormBodyHtml: function(cellName, webpart) {
			// vrací html, které je v editační části řádku s display name cellname
			return this.getFormBody(cellName, webpart).html();
		},

		putValueInInput: function(fieldDisplayName, val, overwrite) { // TODO dodelat SPFieldMultiChoice a hledat podle FieldTypeu a ne trid
			// do řádku ve form, identifikovaného v fieldDisplayName, vloží hodnotu val
			var editForm = this.getFormBody(fieldDisplayName);
			val = $.trim(val);

			//var originalFocus = $("*:focus");//vraci empty []
			var originalFocus = document.activeElement;

			// je to textove pole - nevyplnovat, pokud uz tam neco je (byl postback), pokud neni uvedeno jinak
			var run = function () {
				var elem = editForm.find("textarea.ms-long");

				if (elem.length == 1 && ($(elem).val() == "" || overwrite)) {
					$(elem).val(val).change();
					window.top.console.log("putValueInInput 1: ", fieldDisplayName, " - ", val);
					return;
				}

				elem = editForm.find("input.ms-long");

				if (elem.length == 1 && ($(elem).val() == "" || overwrite)) {
					$(elem).val(val).change();
					window.top.console.log("putValueInInput 2: ", fieldDisplayName, " - ", val);
					return;
				}

				elem = editForm.find("input.ms-input");

				if (elem.length == 1 && ($(elem).val() == "" || overwrite)) {
					$(elem).val(val).change();
					window.top.console.log("putValueInInput 3: ", fieldDisplayName, " - ", val);
					return;
				}

				// rich text editor
				elem = editForm.find("iframe.ms-rtelong");

				if (elem && elem.length == 1) {
					$(frames[$(elem).attr("id")].document, elem).find("body").append("<div>" + val.replace("%0A", "<br />") + "</div>");
					window.top.console.log("putValueInInput 4: ", fieldDisplayName, " - ", val);
					return;
				}

				// enhanced rich text editor
				elem = editForm.find(".ms-rtestate-write p");

				if (elem && elem.length == 1) {
					elem.html(val.replace("%0A", "<br />"));
					window.top.console.log("putValueInInput 10: ", fieldDisplayName, " - ", val);
					return;
				}

				// je to lookup v IE
				elem = editForm.find(".ms-lookuptypeintextbox");

				if (elem.length == 1) {
					var selectBox = $(elem[0]).val(val).next("img").trigger("click").next("select").get(0);

					if (selectBox && selectBox.options) {
						for (var i = 0; i < selectBox.options.length; i++) {
							if (selectBox.options[i].text == val) {
								selectBox.selectedIndex = i;
								$(selectBox).focusout().change();
								window.top.console.log("putValueInInput 5: ", fieldDisplayName, " - ", val);
								break;
							}
						}
					}

					return;
				}

				// je to choice s dropdownlistem / lookup - pozor, lookup v IE neni jen dropdownlist a je potreba osetrit vic
				elem = editForm.find("select");
				if (elem.length == 1) {
					for (var i = 0; i < elem[0].options.length; i++) {
						if (elem[0].options[i].text == val || elem[0].options[i].value == val) {
							elem[0].selectedIndex = i;
							$(elem[0]).dblclick();
							window.top.console.log("putValueInInput 6: ", fieldDisplayName, " - ", val);
							break;
						}
					}
					return;
				}

				// je to multilookup - TODO detekovat postback? nemelo by vadit
				if (elem.length == 2) {
					var selector = editForm.find("[id$='MultiLookup']");
					var pickOptionAndSelect = function () {
						/*$(this).dblclick();
						editForm.find('input[id$=\"_AddButton\"]').prop('disabled', false).click();*/
						var multilookupPickerVal = $(selector).val();
						if ($(selector).val() == undefined || $(selector).val().length == 0) {
							$(selector).val($(this).val() + "|t" + $(this).text());
						}
						else {
							$(selector).val(multilookupPickerVal + "|t" + $(this).val() + "|t" + $(this).text());
						}
					};

					if (!isNumber(val)) {
						$(elem[0]).find("option[title='" + val + "']").each(pickOptionAndSelect).appendTo(elem[1]);
					} else {
						$(elem[0]).find("option[value='" + val + "']").each(pickOptionAndSelect).appendTo(elem[1]);
					}
					window.top.console.log("putValueInInput 7: ", fieldDisplayName, " - ", val);

					return;
				}

				// je to userfield nebo nejake jine pole - nevyplnovat, pokud uz tam neco je (byl postback), pokud neni uvedeno jinak
				// verze pro IE
				//http://www.sharepointcolumn.com/sp2013-setting-people-picker-value-in-newform-aspx/
				elem = editForm.find("div.sp-peoplepicker-topLevel");
				/*
				Better way of disabling field is
				spPeoplePicker.SetEnabledState(false); and to hide X image you can use this
				$(‘.sp-peoplepicker-delImage’).css(‘display’,'none’);
				*/

				if (elem.length == 1) {
					// IE9 nacita veskere onLoaded,atd... az pri F5, proto timeout
					function isIE() {
						var myNav = navigator.userAgent.toLowerCase();
						return (myNav.indexOf('msie') != -1) ? parseInt(myNav.split('msie')[1]) : false;
					}
					function ppHandler() {
						var ppdiv = elem;
						var ppinst = SPClientPeoplePicker.SPClientPeoplePickerDict[ppdiv[0].id];
						var ppinput = ppdiv.find(".sp-peoplepicker-editorInput");
						var splitUsers = val.split(";");
						for (var i=0; i < splitUsers.length; i++) {
							ppinput.val(splitUsers[i]);
							ppinst.AddUnresolvedUserFromEditor(true);
						}
						ppdiv.find('.sp-peoplepicker-delImage').css('display', 'none');
						window.top.console.log("putValueInInput 8: ", fieldDisplayName, " - ", val);
						$(originalFocus).focus();
					}
					if (isIE() > 9) {
						setTimeout(ppHandler, 2000);
					} else {
						$(window).load(ppHandler);
					}
					return;
				}

				elem = editForm.find("textarea.ms-input");

				if (elem.length == 1 && ($(elem).val() == "" || overwrite)) {
					$(elem).val(val).change();
					window.top.console.log("putValueInInput 9: ", fieldDisplayName, " - ", val);
					return;
				}
			};
			try {
				run();
			} catch (e) {
				window.top.console.log("PutValueInInput failed:", fieldDisplayName, " - ", val, "\n", e);
			}
			$(originalFocus).focus();
		}
	};
	
	return NVR;
});