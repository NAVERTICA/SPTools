define(['jquery', 'underscore', 'nvr', 'nvr_general', 'nvr_common', 'nvr_notifications'/*,...*/], function ($, _, NVR/*,...*/) {
	"use strict";
	
	NVR.listView = {
		clearListViewPage: function(wpId){
			$('<div ID="formAppendPoint"></div>').appendTo("form");
			$(".ms-listviewtable script").remove();

			if (!NVR.general.isListViewPage()){ //mame webpartu na page
				$("td[id*='" + wpId + "'].s4-wpActive").append($("<hr>")).appendTo("#formAppendPoint");
				$(".ms-formtable").appendTo("#formAppendPoint");
				$("body").css("overflow", "scroll");
				$("form").children("[id!=formAppendPoint]").hide(); // SP2010
				$(".ms-addnew").hide();
				$("hr").hide();
				var width = screen.width;
				$("#formAppendPoint td[id*='" + wpId + "']").css({ "width": "" + width + "" });
			}
			else{ //webparta na formualari v listu /knihovne
				$(".ms-listviewtable").append($("<hr>")).appendTo("#formAppendPoint");
				$(".ms-formtable").appendTo("#formAppendPoint");
				$("body").css("overflow", "scroll");
				$("form").children("[id!=formAppendPoint]").hide(); // SP2010
				$("hr").hide();
			}
		},
		jqListRows: function(webpart){
			// vraci jquery objekty - vsechny radky v pohledu, pripadne v casti pohledu upresnene webpart
			// TODO - zrejme nebude fungovat na Views jen se dvema sloupci, protoze .ms-vb2 se objevi az pri trech (je potreba overit)
			if (!webpart) {
				return $(".ms-listviewtable tbody:visible > tr:has(.ms-vb2):not(:has(th))");
			}
			else return $(webpart).find("tbody:visible > tr:has(.ms-vb2):not(:has(th))");
		},
		getListRows: function(webpart){
			// {{{ // SPT2010 OK
			// vrati pole s radky polozkami aktualniho seznamu - polozka ma ID, coz je ID radku (NE poradove cislo)
			// row, coz je odkaz na samotny radek (tr), a contents, coz je html v radku
			var listItems = [];
			var index = 0;
			var rows;
			var colId = -1;

			rows = this.jqListRows(webpart);
			colId = this.getColIndexByName("ID", null, webpart);

			rows.each(function () {
				var item = {};
				if (colId != -1) {
					var txt = $(this).contents().eq(colId).text();
					if ($.trim(txt) != "") {
						item.ID = $.trim(txt);
					}
				}
				else {
					item.ID = $(this).children("td.ms-vb-title")
									 .children()
									 .attr("Id");

					if (!item.ID) {
						var hrefId = $(this).children(".ms-vb2").children("a[href*='ID=']").attr("href");
						if (hrefId) {
							hrefId = hrefId.split("ID=")[1];
							item.ID = hrefId.split("&")[0];
						}
					}
				}

				item.webpart = webpart;
				item.row = $(this);
				item.contents = $(this).contents();
				item.rowID = index;
				listItems.push(item);
				index += 1;
			});
			return listItems;
		},
		jqListHeaders: function(webpart){
			// {{{ // SPT2010 OK
			// vraci jquery objekty - vsechny hlavicky sloupcu v pohledu, pripadne v casti pohledu upresnene webpart
			if (!webpart) {
				return $(".ms-listviewtable tr.ms-viewheadertr th, .ms-emptyView tr.ms-viewheadertr th");
			}
			else return $(webpart).find("tr.ms-viewheadertr th");
		},
		getListHeaders: function(webpart, includeAll){
			// {{{ // SPT2010 OK
			// ziska pole se slovniky postavene podle hlavicek sloupcu
			// pokud je zadouci zahrnout "systemove" sloupce s .ms-vh-icon, je potreba predat parametr includeAll = true
			// slovnik obsahuje colindex, name (jmeno sloupce jak je videt), fieldtype a pripadne intname, pokud jde sehnat
			var listHeaders = [];
			var i = 0;
			var headers;

			headers = this.jqListHeaders(webpart);

			headers.each(function () {
				if ($(this).hasClass("ms-vh-icon") && !includeAll) {
					i++;
					return;
				}
				var item = {
					colindex: i,
					intname: $(this).find("div").first().attr("name"),
					fieldtype: $(this).find("div").first().attr("fieldtype"),
					name: $.trim(nbspReplace($(this).text())) // v IE z nejakeho duvodu ani pomoci jQuery nesly trimovat nbsp (charcode 160/A0)
				};

				listHeaders.push(item);

				i++;
			});

			return listHeaders;
		},
		getColIndexByName: function(colname, headers, webpart){
			 // {{{ // SPT2010 OK
			// vrati cislo sloupce se jmenem colname, kdyz nenajde, vraci -1
			// headers muze byt null, pouzije se pak getListHeaders (pripadne s webpart)
			var colindex = -1;
			if (typeOf(colname) == "number")
				return colname;

			if (!headers) {
				headers = this.getListHeaders(webpart, true);
			}

			if (typeOf(colname) == "array") {
				for (var j = 0; j < colname.length; j++) {
					var r = this.getColIndexByName(colname[j], headers, webpart);
					if (r != -1) {
						colindex = r;
						break;
					}
				}
			} else {
				for (var i = 0; i < headers.length; i++) {
					if ($.trim(nbspReplace(headers[i].name)) == colname || headers[i].intname == colname) {
						colindex = i;
						break;
					}
				}
			}

			if (colindex != -1) {
				return colindex + this._hasFolders(webpart);
			}
			return -1;
		},
		getCell: function(row, col, webpart){
			// row je items[i] nebo items[i].contents, col je index nebo jmeno sloupce, pokud je to jmeno, muze byt potreba parametr webpart
			// pokud sloupec (ani jeden) neexistuje, vraci prazdny jquery objekt
			var colIndex = this.getColIndexByName(col, null, webpart);

			if (colIndex == -1) return $();

			if (typeOf(row) == "number" || typeOf(row) == "string") {
				var rows = this.getListRows();
				for (var i = 0; i < rows.length; i++) {
					if (rows[i].ID == row + "") {
						row = rows[i];
						break;
					}
				}
			}

			if (!row.contents) {
				return $(row).eq(colIndex); // TODO  - tato varianta nefunguje (kdyz do fce predame primo row.contents)
			}
			else {
				return $(row.contents).eq(colIndex);
			}
		},
		getCellValue: function(row, col, webpart){
			return this.getCell(row, col, webpart).html();
		},
		getCellHtml: function(row, col, webpart){
			return this.getCell(row, col, webpart).html();
		},
		getCellText: function(row, col, webpart){
			return this.getCell(row, col, webpart).text();
		},
		getCellContents: function(row, col, webpart){
			return this.getCell(row, col, webpart).contents();
		},
		// TODO v IE vraci pri spatnem nazvu vsechny bunky
		getColCells: function(colname, webpart){
			// vrací všechny buňky sloupce daného jména
			var items = null;
			var pos = -1;
			items = this.jqListRows(webpart);

			var selectorItems = [];
			var selector = "";
			if (!(colname instanceof Array)) {
				colname = [colname];
			}

			_.each(colname, function (col) {
				pos = NVR.listView.getColIndexByName(col, null, webpart);
				if (pos == -1) {
					return;
				}
				selectorItems.push("td:nth-child(" + (pos + 1) + ")"); // TODO otestovat folder depth
			});
			selector = selectorItems.join(", ");

			if (selector == "") {
				return $([]);
			}

			return $(items).children(selector);
		},
		eachColCell: function(colname, webpart, func){
			// pro každou buňku ve sloupci/sloupcích colname provede func, this ve funkci bude buňka
			this.getColCells(colname, webpart).each(func);
		},
		colCellToHtml: function(colname, webpart){
			// textový obsah každé buňky ve sloupci / sloupcích převede na html
			this.getColCells(colname, webpart).each(function () {
				if (!$(this).attr("processed")) {
					$(this).html($(this).text()).attr("processed", "true");
				}
			});
		},
		appendSourceHref: function(colname, webpart, src, blank){
			// do všech odkazů ve sloupci daného jména doplní parametr Source, defaultně bude Source = window.location, pokud blank==True, bude se odkaz otevírat v novém okně
			var src2 = window.location;
			if (src) {
				src2 = src;
			}
			this.getColCells(colname, webpart).find("a").each(function(){
				var link = $(this).attr("href");
				link = appendToUrl(link, "Source=" + src2);
				$(this).attr("href", link);

				if (blank) {
					$(this).attr("target", "_blank");
				}
			});
		},
		enableCluetip: function(){
			// dotáhne do stránky tooltip knihovny
			$("head").append("<script type='text/javascript' src='/_layouts/15/SPTools/libs/jquery.cluetip.js'></script>");
		},

		//depends on ContextGuids
		attachmentsPopup: function(webpart){
			// ikonky příloh se v seznamu aktivují a po kliknutí ukážou seznam příloh
			if (!$.cluetip) {
				this.enableCluetip();
			}

			var rows = this.getListRows(webpart);

			$.each(rows, function () {
				$(this.contents).find('img[src$=attach.gif]').attr('border', '0').attr('alt', '')
					.wrap('<a href="#" rel="/_layouts/15/SPTools/AttachmentsView.aspx?WebId='
							+ ContextGuids.web + '&List=' + ContextGuids.list + '&Id=' + this.ID + '" class="cluetip"></a>');
				$(this.contents).find('a.cluetip').cluetip({
					cluetipClass: 'default',
					mouseOutClose: true,
					showTitle: false,
					sticky: true,
					dropShadow: false,
					closeText: '', //'<img src="/_layouts/15/images/EXITEDIT.gif" alt="zavřít" border="0" />',
					closePosition: 'top',
					activation: 'click'
				});
			});
		},
		colHrefToModal: function(){
			// pro dodaná jména sloupců (string nebo string array), případně při více seznamech v jednom pohledu upřesněné parametrem webpart
			// nastaví onclick pro tyto sloupce tak, aby se otevíraly v modálním okně na stejné stránce

			this.getColCells(colname, webpart)
				.find("a")
				.each(function () {
					$(this).unbind("click").bind("click", null, function () {
						var url = $(this).attr("href");
						url = NVR.general.appendToUrl(url, "IsDlg=1");
						NVR.form.openDialog(url);
						return false;
					});
				});
		},
		bindColName: function(ev, func, colname, additionaldata, headers, items, webpart){
			// navaze na bunky ve sloupci se jmenem colname funkci func, spustenou na udalost ev
			// predtim provede unbind na ev
			// headers i items muze byt null, pouzije se getListHeaders/getListRows
			// pri chybe vraci false, jinak true
			if (!headers) {
				headers = this.getListHeaders(webpart);
			}

			if (!items) {
				items = this.getListRows(webpart);
			}
			if (!additionaldata) {
				additionaldata = {};
			}

			var pos = this.getColIndexByName(colname, headers, webpart);

			if (pos == -1) {
				return false;
			}

			// melo by byt zahrnuto uz v getColIndexByName
			// var folderDepth = _hasFolders();
			// pos = pos + folderDepth;

			_.each(items, function (item, i) {
				var rowItems = item.row.children();
				var data = {};
				_.each(additionaldata, function (value, key) {
					data[key] = value;
				});

				data.ID = item.ID;
				data.rowID = i;

				data.pos = pos;

				data.colName = headers[pos - folderDepth].name;

				$(rowItems[pos]).unbind(ev);
				$(rowItems[pos]).bind(ev, data, func);
			});
			return true;
		},
		_hasFolders: function(webpart){
			var result = 0;
			if (!webpart) {
				// vnorene seskupeni
				if ($("td .ms-gb2").length > 0) {
					result = 2;
				}
					// jednoduche
				else if ($("td .ms-gb").length > 0 || $("td .ms-vh-group").length > 0) {
					result = 1;
				}
			}
			else {
				// vnorene seskupeni
				if ($(webpart).find("td .ms-gb2").length > 0) {
					result = 2;
				}
					// jednoduche
				else if ($(webpart).find("td .ms-gb").length > 0 || $(webpart).find("td .ms-vh-group").length > 0) {
					result = 1;
				}
			}
			return result;
		},
		addColAtPos: function(contentfunc, pos, name, items, headers, webpart){
			// contenetfunc musi vracet string, ktery bude obsahem noveho sloupce, jako parametr dostane radek z items
			// pos je pozice, name = jmeno sloupce, items muzou byt null
			//try {
			if (!items) {
				items = this.getListRows(webpart);
			}

			var headerItems = null;
			if (!headers) {
				headerItems = this.jqListHeaders(webpart);
			}
			else {
				headerItems = headers;
			}

			var colindex = this.getColIndexByName(name, null, webpart);

			if (colindex == -1) {
				var th = $('<th scope="col" class="ms-vh2" nowrap="nowrap"><div class="ms-vh-div">' + name + '<img src="/_layouts/15/images/blank.gif" alt="Filtr" border="0"></a></th>');

				if (pos >= headerItems.length) {
					var index = headerItems.length - 1;

					if ($(headerItems[index]).hasClass("ms-vh-icon")) index = index - 1;

					th.insertAfter(headerItems[index]);
					colindex = headerItems.length;
				} else {
					th.insertBefore(headerItems[pos]);
					colindex = pos;
				}

				headerItems = this.jqListHeaders(webpart);
			}

			_.each(items, function (item) {
				var rowItems = item.row.children();

				var td = $("<td id='rowID" + item.ID + "' class='ms-vb2'></td>");
				var tdContent = contentfunc(item);
				td.append(tdContent);

				if (!rowItems[headerItems.length]) {
					while (colindex > 0) {
						try {
							if (rowItems[colindex]) {
								$(rowItems[colindex]).after(td);
								break;
							}
							colindex = colindex - 1;
						} catch (E) {
							window.top.console.error(tdContent, E);
						}
					}
				} else {
					$(rowItems[colindex]).replaceWith(td);
				}
			});
		},
		addCols: function(noveSloupce, skrytSloupce, webpart){
			// skrytsloupce je string array, sloupce s tímto jménem se skryjí
			try {
				//$.blockUI();
				if (!webpart) {
					webpart = $(".ms-listviewtable").parents("div[id*='WebPartWPQ']");
				}

				var addNewHref = $("a.ms-addnew").attr("href");
				addNewHref += "&Source=" + GetSource();

				$("a.ms-addnew").attr("href", addNewHref);

				// vykuchat obsah funkce, ktera dela refresh samotne webparty
				var doPostback = "";
				var hrefScripts = webpart.find("div[WebPartID]").find("a[href*=__doPostBack][href*=dvt_sortfield]");
				window.top.console.log("hrefScripts", hrefScripts);
				var myregexp = /javascript:[\s]*(__doPostBack\('[a-zA-Z0-9$_]*').*/;

				for (var i = 0; i < hrefScripts.length; i++) {
					var hrefContents = $(hrefScripts[i]).attr("href");
					var match = myregexp.exec(hrefContents);
					if (match != null) {
						doPostback = match[1];
						break;
					}
				}

				if (doPostback != "") {
					doPostback += ");";
					doPostback = doPostback.replace(/'/g, '"');
				}

				var headerItems = this.jqListHeaders(webpart);
				var items = this.getListRows(webpart);

				_.each(noveSloupce, function (sloupec) {
					NVR.listView.addColAtPos(function (item) {
						try {
							if (sloupec.hideConditions) {
								if (typeOf(sloupec.hideConditions) == "function") {
									if (sloupec.hideConditions(item)) return "";
								} else if (typeOf(sloupec.hideConditions) == "object") {
									// pokud radek v nekterem poli uvedenem v hideConditions obsahuje hodnotu odpovidajici klici, nic v nem nezobrazime
									for (colname in sloupec.hideConditions) {
										var celltext = getCellText(item, colname);

										if (!(sloupec.hideConditions[colname] instanceof Array)) {
											sloupec.hideConditions[colname] = [sloupec.hideConditions[colname]];
										}

										for (var q = 0; q < sloupec.hideConditions[colname].length; q++) {
											if (celltext == sloupec.hideConditions[colname][q]) {
												return "";
											}
										}
									}
								}
							}

							// nacist id seznamu pro cookie budto z sloupec (polozka noveSloupce) nebo z obsahu skriptu uvnitr webparty
							// v IE je nutno pouzit html() a jeste k tomu projit kazdy vyskyt zvlast
							var webpartListId = "";
							if (sloupec.cookieId) {
								webpartListId = sloupec.cookieId.toString().toLowerCase();
							} else {
								var scriptTags = webpart.find("div[WebPartID]").find("script");
								for (var x = 0; x < scriptTags.length; x++) {
									var scriptContents = $(scriptTags[x]).html();
									var index = scriptContents.indexOf("toolbarData['ListId']=") + 23;
									if (index > 22) {
										webpartListId = scriptContents.substring(index, index + 36).toLowerCase();
										break;
									}
								}
							}

							// vyplnime si cookieElements, ale pak ho pouzijeme az pri onclick jako text a tim ulozime aktualni cookie
							var cookieElements = {};
							var key;
							if (typeOf(sloupec.prefillSettings) == "object") {
								for (key in sloupec.prefillSettings) {
									if (key) cookieElements[key] = getCellText(item, sloupec.prefillSettings[key]);
								}
							}

							if (typeOf(sloupec.additionalPrefill) == "object") {
								for (key in sloupec.additionalPrefill) {
									cookieElements[key] = sloupec.additionalPrefill[key];
								}
							}

							var result;
							if (typeOf(sloupec.prefillFunction) == "function") {
								result = sloupec.prefillFunction(item);
								if (typeOf(result) == "object") {
									for (key in result) {
										if (key) cookieElements[key] = result[key];
									}
								}
							}

							// radky, ktere ma prefill schovat
							if (typeOf(sloupec.hideFormRows) == "array" && sloupec.hideFormRows.length > 0) {
								cookieElements["_HideAlways"] = sloupec.hideFormRows.join(";");
							}

							if (typeOf(sloupec.hidePrefilledFormRows) == "array" && sloupec.hidePrefilledFormRows.length > 0) {
								cookieElements["_HidePrefilled"] = sloupec.hidePrefilledFormRows.join(";");
							}

							// parametry do url
							var urlElements = new Array();

							if (typeOf(sloupec.urlParamFunction) == "function") {
								result = sloupec.urlParamFunction(item);
								if (typeof result == "array") {
									for (var j = 0; j < result.length; j++) {
										urlElements.push(result[j]);
									}
								} else if (typeOf(result) == "object") {
									for (key in result) {
										urlElements.push(key + "=" + sloupec.urlParameters[key]);
									}
								} else if (typeOf(result) == "string") {
									urlElements.push(result);
								}
							}

							if (sloupec.urlParameters) {
								if (typeOf(sloupec.urlParameters) == "array") {
									for (var j = 0; j < sloupec.urlParameters.length; j++) {
										urlElements.push(sloupec.urlParameters[j]);
									}
								} else if (typeOf(sloupec.urlParameters) == "object") {
									for (key in sloupec.urlParameters) {
										urlElements.push(key + "=" + sloupec.urlParameters[key]);
									}
								} else if (typeOf(sloupec.urlParameters) == "string") {
									urlElements.push(sloupec.urlParameters);
								}
							}

							if (sloupec.idToUrl) {
								urlElements.push("ID=" + item.ID);
							}

							var urlString = urlElements.join("&");
							window.top.console.log("urlString", urlString);

							// cil oteviraneho odkazu
							var target = "";
							if (sloupec.target) {
								target = sloupec.target;
							}

							// obsah sloupce
							var textContent = "";
							if (typeOf(sloupec.content) == "string") {
								textContent = sloupec.content;
							} else if (typeOf(sloupec.content) == "function") {
								textContent = sloupec.content(item);
							}

							// pokud nemame basicUrl, vratime proste obsah textContent
							if (!sloupec.basicUrl) {
								return $(textContent);
							}

							var url = "";
							if (typeOf(sloupec.basicUrl) == "string") {
								url = sloupec.basicUrl;
							} else if (typeOf(sloupec.basicUrl) == "function") {
								url = sloupec.basicUrl(item);
							}
							var str = "<a " + target
								+ "onclick='setPrefillCookie(" + JSON.stringify(cookieElements) + ", \"" + webpartListId + "\"); "
								+ ((typeOf(sloupec.dialog) == "undefined" || sloupec.dialog)
									? ("openDialog(\"" + appendToUrl(url, urlString) + "\", function(res,val) {" + doPostback + "}); return false;") : "")
								+ "' "
								+ "href='" + appendToUrl(url, urlString) + "'>"
								+ textContent
								+ "</a>";
							window.top.console.log('adding cell', str);
							return $(str);
						} catch (exc) {
							window.top.console.error(exc);
						}
					},
							sloupec.position,
							sloupec.name,
							items,
							headerItems,
							webpart);
				});

				for (var ww = 0; ww < skrytSloupce.length; ww++) {
					this.hideColumn(skrytSloupce[ww], webpart);
				}
			} finally {
				//$.unblockUI();
			}
		},
		hideColumn: function(columnIndex2, webparts){
			// skryje ze seznamu sloupec nebo sloupce, columnIndex2 může být string nebo string array, jqHeaders můžou být null,
			// webpart může být null nebo určovat se kterým zobrazeným seznamem pracujeme
			var headers = null;
			var columns = null;
			var webpart = null;

			if (columnIndex2 instanceof Array) {
				columns = columnIndex2;
			} else {
				columns = [columnIndex2];
			}

			if (!webparts) {
				webparts = [null];
			} else {
				webparts = $(webparts);
			}

			$.each(webparts, function (index, webpart) {
				for (var i = 0; i < columns.length; i++) {
					if (!webpart) {
						headers = NVR.listView.jqListHeaders();
					}
					else {
						headers = NVR.listView.jqListHeaders(webpart);
					}

					if (typeOf(columns[i]) == "string") {
						if (!webpart) {
							columns[i] = NVR.listView.getColIndexByName(columns[i], null);
						} else {
							columns[i] = NVR.listView.getColIndexByName(columns[i], null, webpart);
						}
					}

					var no_of_folders = 0;
					// uz by melo byt zahrnuto v GetColIndexByName
					/*if (!webpart) {
						no_of_folders = _hasFolders();
					} else {
					   no_of_folders = _hasFolders(webpart);
					}*/

					if (typeOf(columns[i]) == "number" && columns[i] != -1) {
						$(headers).eq(columns[i] - no_of_folders).hide();

						if (!webpart) {
							NVR.listView.getColCells(columns[i]).hide();
						}
						else {
							NVR.listView.getColCells(columns[i], webpart).hide();
						}
					}
				}
			});
		},
		// funguje jen pro stranky v 2007 zobrazeni
		addListToolbarButton: function(text, onclickFunc){
			// do toolbaru v zobrazení seznamu přidá tlačítko, po kliknutí zavolá funkci
			var htmlToAppend =
					 "<td class=ms-separator><img src='/_layouts/15/images/blank.gif' alt=''></td>\n"
					+ "<td class='ms-toolbar' nowrap='true'><span style='display:none'></span>\n"
					+ "<span><div id='tbButton_" + text.replace(" ", "_") + "' class='ms-menubuttoninactivehover'"
					+ "onmouseover='MMU_PopMenuIfShowing(this);MMU_EcbTableMouseOverOut(this, true)'"
					+ "hoverActive='ms-menubuttonactivehover' hoverInactive='ms-menubuttoninactivehover'"
					+ "nowrap='nowrap'><a id='#tbA_" + text.replace(" ", "_") + "' href='#' onclick='javascript:return false;' "
					+ "style='cursor:pointer;white-space:nowrap;' >" + text + "<img src='/_layouts/15/images/blank.gif' border='0' /></a>"
					+ '</div></span></td>';

			$(".ms-menutoolbar .ms-toolbar[width='99%']").eq(0).before(htmlToAppend);

			$("#tbButton_" + text.replace(" ", "_")).bind("click", onclickFunc);
		},
		ShowSimpleServiceCallResult: function(url, data, statusTitle){
			$.ajax({
				"url": url,
				"success": function (data, textStatus) {
					NVR.notifications.showStatus(statusTitle, data, "blue", true, null);
				},
				"error": function (XMLHttpRequest, textStatus, errorThrown) {
					NVR.general.reportError(textStatus + " - " + errorThrown, true);
				},
				"dataType": "text"
			});
		},
		anyItemsSelected: function(){
			return SP.ListOperation.Selection.getSelectedItems().length > 0;
		},
		getSelectedItemIds: function(){
			var ids = [];
			var selectedItems = SP.ListOperation.Selection.getSelectedItems();
			for (k in selectedItems) {
				ids.push(selectedItems[k].id);
			}
			return ids;
		},
		// TODO 2010 - nejede
		columnsForSelector: function(selector, webpart2){
			// vrací sloupce, jejichž tabulka má určitý selector - pokud je selector "[fieldtype=Calculated]", hledaný selector bude "table[fieldtype=Calculated]"
			var indices = [];
			var webpart = null;

			if (webpart2) {
				webpart = $(webpart2);
			} else {
				webpart = $("table.ms-listviewtable");
			}

			$(webpart).find("table" + selector).each(function () {
				indices.push(NVR.listView.getColIndexByName($(this).text(), null, webpart));
			});

			return indices;
		},
		initCalculatedHtml: function(colIndex, webpart2){
			// v daném sloupci převede text na html, tzn. projeví se html kód ve vypočítaných polích
			var webpart = null;
			if (webpart2) {
				webpart = $(webpart2);
			} else {
				webpart = $("table.ms-listviewtable");
			}

			this.getColCells(colIndex, webpart).each(function () {
				var inside = $(this).text().toLowerCase();
				if (inside.indexOf("<div") == 0 && inside.lastIndexOf("</div>") > (inside.length - 7)) { // začíná divem a na konci ho uzavírá
					$(this).html($(this).text()); // text v obsahu bude zobrazen jako html
				}
			});
		}
	};
	
	return NVR;
});