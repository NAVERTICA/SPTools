define(['jquery', 'underscore', 'nvr', 'nvr_e_n_d_form'/*,...*/], function ($, _, NVR/*,...*/) {
	"use strict";
	
	NVR.aggregator = {
		vytvoritGrid: function(_GridID, _colmodel, _ukoly, _days, _rowNum, enableFilter) {
			$("#" + _GridID).jqGrid({
				datatype: "local",
				height: "auto",
				colModel: _colmodel,
				rowNum: _rowNum, // -1 = ALL - v novejsim jQuery uz nefunguje
				altRows: true,
				autowidth: true,
				ignoreCase: true, //case insensitive search
				multiselect: false,
				sortorder: 'asc',
				loadui: 'disable',
				loadComplete: function () // vola se po kazdem nacteni gridu, i pri sortovani
				{
					try {
						if (NVR.aggregatorLoaded) // funkce na dueDateControlDays - zatim
						{
						    NVR.aggregatorLoaded(_GridID, _ukoly, _days);
						}
					} catch (e) {
						if (e.toString().indexOf("aggregatorLoaded' is undefined") == -1) {
							//console.log("aggregatorLoaded - " + e.toString());
							//alert("aggregatorLoaded - " + e.toString());
						}
					}
				}
			});
			if (enableFilter) {
				$("#" + _GridID).jqGrid('filterToolbar', {
					searchOnEnter: true
					, stringResult: true
					, defaultSearch: 'cn'
				});
			}
		},

		vytvoritTreeGrid: function(_GridID, _colmodel, _ext, _expandColumn, sirka) { //depends on ContextGuids, jqgrid

			var Source = window.location.href.split('?')[0];

		    $(function() {
		        var wli = ContextGuids["item"] != undefined && ContextGuids["itemTitle"] != undefined ? ContextGuids["web"] + ":" + ContextGuids["list"] + ":" + ContextGuids["item"].ID : "";
		        var grid = $("#" + _GridID);
		        grid.jqGrid({
		            url: "/_layouts/15/SPTools/AggregatorTreeGridProvider.aspx?Source=" + Source + "&extension=" + _ext + "&isForm=" + isFormPage() + "&wli=" + wli,
		            treedatatype: "xml",
		            mtype: "POST",
		            colModel: _colmodel,
		            treeGrid: true,
		            treeGridModel: 'nested',
		            ExpandColumn: _expandColumn,
		            height: 'auto',
		            autowidth: true,
		            ignoreCase: true, //case insensitive search
		            loadui: "disable",
		            gridComplete: function() {
		                expandNodes();

		                if (isFormPage()) //Select row in displayForm
		                {
		                    var url = NVR.Url.decode(window.location.href);
		                    var listName = "";
		                    var rets = url.split('/');
		                    for (var i = 0; i < rets.length; i++) {
		                        if (rets[i] == "Lists") {
		                            listName = rets[i + 1];
		                            break;
		                        }
		                    }

		                    var trID = $(".ui-jqgrid-btable tr td[title*='" + NVR.e_n_d_form.getFormBodyText('Title') + "'] a[href*='" + ContextGuids.list + "'][href*='ID=" + ContextGuids.item.ID + "']").parent().parent().parent().attr("id");
		                    if (trID != undefined) {
		                        $("#" + _GridID).setSelection(trID, true);
		                    }
		                }
		                if (sirka > 0) {
		                    $("#" + _GridID).setGridWidth(sirka, true);
		                }
		            }
		        });

		        function expandNodes() {
		            function repeatingBackupStuff() {
		                var rowEx = $("table#" + _GridID + " .tree-plus").eq(0);
		                //window.top.console.info("Prvku "+$(".treeclick","table#GroupHierarchyGrid").length);
		                //window.top.console.info("rowEx:"+$(".treeclick","table#GroupHierarchyGrid").eq(0).length);
		                if (rowEx.length > 0) {
		                    rowEx.trigger("click");
		                } else {
		                    setTimeout(repeatingBackupStuff, 20);
		                }
		            }

		            setTimeout(repeatingBackupStuff, 20);
		        }
		    });
		},

		nahratUkoly: function(_GridID, ukoly, _orderBy) {
			$("#" + _GridID).jqGrid('clearGridData');
			if (ukoly) {
				for (var i = 0; i < ukoly.length; i++) {
					if (ukoly[i]["WorkflowLink"]) {
						ukoly[i]["WorkflowLink"] = "<a href='" + ukoly[i]["WorkflowLink"] + "'>link</a>";
					}

					if (ukoly[i]["ListUrl"]) {
						var lastSlash = ukoly[i]["ListUrl"].lastIndexOf("/");
						// z "/Lists/Tasks/AllItems.aspx" udela "ListsTasks"
						var listUrl = ukoly[i]["ListUrl"].substring(0, lastSlash).replace(/\//g, "");

						ukoly[i]["SPID"] = listUrl + "_" + ukoly[i]["ID"];
					} else {
						ukoly[i]["SPID"] = ukoly[i]["ID"];
					}

					/*if (ukoly[i]["Author"]) {
						var author = ukoly[i]["Author"];
						ukoly[i]["Author"] = author.substring(author.indexOf("#") + 1, author.length);
					}*/

					$("#" + _GridID).jqGrid('addRowData', i + 1, ukoly[i]); // Vola metodu addRowData v gridu s nejakymi parametry.
				}

				$("#" + _GridID).jqGrid('sortGrid', _orderBy); //TODO
			}
		}
	};
	
	return NVR;
});