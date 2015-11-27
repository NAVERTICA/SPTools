// zaklady: 
// - window.JSONFields[fieldName] pouzivame na zapis objektu s obsahem pole, nalinkovanym editorem atd.
// - window.JSONschema[schemaName] obsahuje funkce, ktere pri zavolani vrati schema - tzn. kazde nacitane schema   
//                                 a podschema do nej zapise svoji funkci... prvni nacitane schema je fieldName
function get_url_param(name, location) {// {{{
	name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
	var regexS = "[\\?&]" + name + "=([^&#]*)";
	var regex = new RegExp(regexS);
	var results = null;
	if (!location) {
		results = regex.exec(window.location.href);
	}
	else {
		results = regex.exec(location);
	}

	if (!results) {
		return "";
	}
	else {
		return results[1];
	}
}
function loadCss(url) {
    var link = document.createElement("link");
    link.type = "text/css";
    link.rel = "stylesheet";
    link.href = url;
    document.getElementsByTagName("head")[0].appendChild(link);
}

(function () {
	window.schemaLink = function (scriptName) {
		return "/SiteScripts/Schema/" + scriptName + ".js";
	};

	// funkce, kterou po vykresleni JSONFieldu zavola ShP
	// zaridi pres require.js modul promenu divu v json-edit podle schematu pojmenovaneho 
	// internim jmenem pole, ktere se snazi najit v /SiteScripts/Schema/IntJmenoPole.js
	function startJSON(fieldName, jsonContents) {
		var jsonContentsParsed = {};
		try {
			jsonContentsParsed = JSON.parse(jsonContents);
		} catch (e) {
			console.error("problem parsing JSON\n", jsonContents, "\n", e);
		}
		loadCss('/_layouts/15/NVR.SPTools/libs/json-edit/css/json.edit.css');
		loadCss('https://ajax.googleapis.com/ajax/libs/jqueryui/1.9.1/themes/smoothness/jquery-ui.css');
		loadCss("/_layouts/15/NVR.SPTools/libs/jquery-ui-multiselect-widget/jquery.multiselect.css");
		loadCss("/_layouts/15/NVR.SPTools/libs/jquery-ui-multiselect-widget/jquery.multiselect.filter.css");
		RegisterSod("require.js", "/_layouts/15/NVR.SPTools/libs/requirejs/require.js");
		RegisterSod("jquery.js", "/_layouts/15/NVR.SPTools/libs/jquery-ui/external/jquery-1.9.1/jquery.js");
		RegisterSod("jqueryui.js", "https://ajax.googleapis.com/ajax/libs/jqueryui/1.9.1/jquery-ui.min.js");
		RegisterSodDep("jqueryui.js","jquery.js");
		RegisterSodDep('require.js', 'jqueryui.js');
		LoadSodByKey("require.js",function(){
		//loadScript("/_layouts/15/NVR.SPTools/libs/requirejs/require.js", function () {
			require.config({
				baseUrl: "js/",
				packages:[{
					name:"codemirror",
					location:"/_layouts/15/NVR.SPTools/libs/CodeMirror",
					main:"lib/codemirror"
				}],
				paths: {
					"json": "/_layouts/15/NVR.SPTools/libs/JSON-js/json2",
					"ace": "/_layouts/15/NVR.SPTools/libs/ace/src-noconflict/ace",
					//"jquery": "/_layouts/15/NVR.SPTools/libs/jquery-ui/external/jquery-1.9.1/jquery",
					"jqueryui": "http://cdnjs.cloudflare.com/ajax/libs/jqueryui/1.10.0/jquery-ui.min",
					"legoparser": "/_layouts/15/NVR.SPTools/libs/legojs/src/legoparser",
					"jquery.lego": "/_layouts/15/NVR.SPTools/libs/legojs/src/jquery.lego",
					"prettyPrint": "https://cdnjs.cloudflare.com/ajax/libs/prettify/r224/prettify",
					"jquery.taghandler": "/_layouts/15/NVR.SPTools/libs/Tag-Handler/js/jquery.taghandler.min",
					"json.edit": "/_layouts/15/NVR.SPTools/libs/json-edit/src/json.edit",
					"json.schema": "/_layouts/15/NVR.SPTools/libs/json-edit/src/json.schema",
					"nsgen": "/_layouts/15/NVR.SPTools/libs/json-edit/src/nsgen",
					//additional libraries
					"Q": "/_layouts/15/NVR.SPTools/libs/q/q",
					"jsonRefs": "/_layouts/15/NVR.SPTools/libs/json-refs/json-refs",
					"SP.RequestExecutor": "/_layouts/15/SP.RequestExecutor",
					//codemirror
					"multiselect":"/_layouts/15/NVR.SPTools/libs/jquery-ui-multiselect-widget/src/jquery.multiselect.min",
					"multiselect.filter":"/_layouts/15/NVR.SPTools/libs/jquery-ui-multiselect-widget/src/jquery.multiselect.filter.min",
					// hints
					"hint.tags": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/tags",
					"hint.autocomplete": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/autocomplete",
					"hint.date": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/date",
					"hint.color": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/color/color",
					"hint.tabs": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/tabs",
					"hint.password": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/password",
					"hint.textarea": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/textarea",
					"hint.readonly": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/readonly",
					"hint.enumlabels": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/enumlabels",
					"hint.tabarray": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/tabarray",
					"hint.summarylist": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/summarylist/addon",
					"hint.adsafe": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/adsafe/adsafe",
					"hint.blockly": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/blockly/blockly",
					"hint.optional": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/optional",
					"hint.codemirror": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/codemirror",
					"hint.htmleditor": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/htmleditor/htmleditor",
					"hint.schGlobalJS": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/scriptholder/schGlobalJS",
					"hint.schWeb": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/scriptholder/schWeb",
					"hint.schList": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/scriptholder/schList",
					"hint.schField": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/scriptholder/schField",
					"hint.schBlockly": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/scriptholder/schBlockly",
					"hint.schApp": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/scriptholder/schApp",
					"hint.schLookup": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/scriptholder/schLookup",
					"hint.schUser": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/scriptholder/schUser",
					"hint.schFieldString": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/scriptholder/schFieldString",
					"hint.schRoles": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/scriptholder/schRoles",
					//async
					"async": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/scriptholder/async",

					// needed by hints
					// by color hint
					"spectrum": "/_layouts/15/NVR.SPTools/libs/spectrum/spectrum",
					"colorPicker": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/color/picker/colorPicker",

					// by adsafe hint
					"jslint": "/_layouts/15/NVR.SPTools/libs/json-edit/src/addons/adsafe/lib/jslint",

					// by summarylist
					"dustjs": "/_layouts/15/NVR.SPTools/libs/dustjs/lib/dust",

					// nvr
					"JSONPath": "/_layouts/15/NVR.SPTools/libs/JSONPath/lib/jsonpath",
					"jQuery-JSONPath": "/_layouts/15/NVR.SPTools/libs/jQuery-JSONPath/lib/jsonpath.jquery",
				},

				shim: {
					json: {
						exports: "JSON"
					},
					ace: {
						exports: "ace" //,
						//deps: ["ace.json", "ace.theme"]
					},
					prettyPrint: {
						exports: "prettyPrint"
					},
					colorPicker: {
						exports: "colorPicker"
					},
					dustjs: {
						exports: "dust"
					}
				}
			});
			define('jquery', [], function () {
				return jQuery;
			});

			var startEditor = function () { console.log("not ready yet"); };

			require([
					"jquery", "json.edit", "ace", "jquery.lego", "prettyPrint", "json", "SP.RequestExecutor",
					"multiselect","multiselect.filter",
					"hint.tags", "hint.autocomplete", "hint.date", "hint.color",
					"hint.tabs", "hint.password", "hint.readonly", "hint.enumlabels",
					"hint.tabarray", "hint.summarylist", "hint.textarea",
					"hint.adsafe", "hint.blockly", "hint.optional", "hint.codemirror",
					"hint.htmleditor", "JSONPath", "jQuery-JSONPath",
					"hint.schGlobalJS", "hint.schWeb", "hint.schList", "hint.schField", "hint.schBlockly", "hint.schApp", "hint.schLookup", "hint.schUser", "hint.schFieldString", "hint.schRoles",
					"async", "Q", "jsonRefs"],
				function ($, mJsonEdit, ace, legojs, prettyPrint, JSON) {
					"use strict";
					//Q: je toto spravny postup?
					window.jsonRefs = require("jsonRefs");
					if (typeof (window.Q) !== "function") window.Q = require("Q");
					//window.schGlobal = require("hint.schGlobalJS");
					
					startEditor = function (id, title, description) {
						var editor,
							schemaEditorId = id + "-out";

						if (!window.JSONFields[id].schema) window.JSONFields[id].schema = {};

						function updateForm(origSchemaValidatedNewDefaults) {
							var editorContents = editor.getSession().getDocument().getValue();
							var formContentsAsNewDefault = null;
							try {
								if (origSchemaValidatedNewDefaults) {
									formContentsAsNewDefault = origSchemaValidatedNewDefaults;
								} else {
									formContentsAsNewDefault = window.JSONFields[id].validate();
								}

							} catch (E) {
								console.log("form contents couldn't be collected (ok on first load), using original defaults", E);
							}

							if (formContentsAsNewDefault) {
								window.JSONFields[id].schema.default = formContentsAsNewDefault;
							} else {
								try {
									window.JSONFields[id].schema.default = JSON.parse(window.JSONFields[id].originalValue);
								} catch (E) {
									console.log("original JSON contents could not be parsed", E);
								}
							}

							var fieldSchemaContents = JSON.stringify(window.JSONFields[id].schema, null, 2);

							if (!editorContents || editorContents != fieldSchemaContents) {
								console.log("using the generated field schema", fieldSchemaContents);
								editor.getSession().getDocument().setValue(fieldSchemaContents, -1);
							} else {
								console.log("using schema from schema editor", editorContents);
								window.JSONFields[id].schema = JSON.parse(editorContents);
							}

							$("#" + schemaEditorId).html("");

							window.JSONFields[id].form = mJsonEdit(schemaEditorId, window.JSONFields[id].schema);
						}

						window.JSONFields[id].update = updateForm;

						function validateForm() {
							var result = window.JSONFields[id].form.collect();
							var errors;
							var data = null;
							if ((result && result.ok) || (result.result && result.result.ok)) {
								data = result.data;
								errors = [];
							} else {
								errors = window.JSONFields[id].form.getErrors(result.result);
							}

							$("#" + id + "-data").text(JSON.stringify(result.data, null, 2));
							$("#" + id + "-validation").text(JSON.stringify(result.result, null, 2));
							prettyPrint();
							console.log(errors);
							return data;
						}

						window.JSONFields[id].validate = validateForm;

						function lego() {
							var L = $.lego({
								"div": {
									"class": "demo-wrapper",
									"$childs": [
										{
											"div": {
												"class": "demo-box",
												"$childs": [
													{
														"div": {
															"id": schemaEditorId,
															"class": "demo-out"
														}
													},
													{
													    "pre": {
													        "id": id,
															"class": "editor",
															"$childs": window.JSONFields[id].schema,
															"style": "height: 100px;"
														}
													}
												]
											}
										}, {
											"div": {
												"class": "demo-actions",
												"$childs": [
													{
														"button": {
															"id": id + "-validate",
															"class": "demo-validate",
															"$childs": "validate"
														}
													}, {
														"button": {
															"id": id + "-run",
															"class": "demo-run",
															"$childs": "run"
														}
													}
												]
											}
										}, {
											"div": {
												"class": "demo-outputs",
												"$childs": [
													{
														"pre": {
															"id": id + "-validation",
															"class": "demo-validation prettyprint"
														}
													},
													{
														"pre": {
															"id": id + "-data",
															"class": "demo-data prettyprint"
														}
													}
												]
											}
										}
									]
								}
							});
							$("#editor" + id).html(L);
						};

						lego();

						$("#" + id + "-validate").off("click").off("submit")
							.click(function () {
								try {
									validateForm();
								} catch (ee) {
									console.log(ee);
								}
								return false;
							});
						$("#" + id + "-run").off("click").off("submit")
							.click(function () {
								try {
									updateForm();
								} catch (ee) {
									console.log(ee);
								}
								return false;
							});

						if (ace) {
							editor = ace.edit(id);
							editor.setTheme("ace/theme/merbivore_soft");
							editor.getSession().setMode("ace/mode/json");

						}

						updateForm();

						return editor;
					};

					window.loadSchema = function (schemaName, contents, force) {
						if (window.JSONschemas[schemaName] && !force) return window.JSONschemas[schemaName];
						var success = true;
						var link = schemaLink(schemaName);
						try {
							console.info("starting to load ", schemaName);
							if (typeof (contents) == "string" && contents.indexOf('"') > -1) contents = JSON.parse(contents);
							$.ajax({
								url: link,
								dataType: "script",
								async: false,
								success: function (scripts, textStatus) {
									if (!window.JSONschemas[schemaName] || !window.JSONschemas[schemaName](contents, schemaName, force)) {
										console.error("error loading schema ", schemaName);
										success = false;
									} else {
										console.info("successfuly loaded ", schemaName);
									}
								},
								error: function (jqXHR, textStatus, errorThrown) {
									console.error("loading schema ", link, "\n", textStatus, errorThrown);
									success = false;
								}
							});
						} catch (e) {
							console.error("error loading ", schemaName, e);
							success = false;
						}

						return success;
					};

					window.loadFieldSchema = function (fldName, contents) {
						var schemaSuccessful = loadSchema(fldName, contents);

						if (!schemaSuccessful || !window.JSONschemas[fldName]) {
							return window.JSONschemas.defaultSchema(contents);
						}

						return window.JSONschemas[fldName](contents);
					};


					window.addSchemaTab = function (self, parentName) {
						// vykuchat interni jmeno pole, ve kterem jsme
						var fldName = $(self).parents('td.ms-formbody').contents()[1].nodeValue.split('\n')[1];
						fldName = fldName.substring(fldName.indexOf('\"') + 1, fldName.lastIndexOf('\"'));

						var name = window.prompt("Name of schema to be added in new tab", "DerivedAction");
						if (!name) return;

						var origSchemaValidatedNewDefaults = window.JSONFields[fldName].validate();
						var schema = window.JSONFields[fldName].schema;
						var result = null;
						//debugger;
						var path = $.JSONPath({ data: schema, keepHistory: false, resultType: "BOTH" });
						if (parentName == "root") {
							result = path.query("$;properties");
						} else {
							result = path.query("$;properties;" + parentName + ";properties");
						}
						try {
							if (loadSchema(name, "")) {
								result[0].value[name] = window.JSONschemas[name]();

								// parent
								var parentQuery = result[0].query.substring(0, result[0].query.lastIndexOf(";properties"));
								var parent = path.query(parentQuery);
								if (!parent[0].value.order) parent[0].value.order = [];
								parent[0].value.order.push(name);
								window.JSONFields[fldName].update(origSchemaValidatedNewDefaults);
							}
						} catch (ae) {
							console.error(ae);
						}
					};

					if (!window.JSONschemas) window.JSONschemas = {};

					// defaultni schema - CodeMirror okno, ve kterem editujeme primo JSON obsah pole
					window.JSONschemas.defaultSchema = function (jsoncontents, force) {
						var sch = {
							"id": "defaultSchema-raw editor",
							"type": "object",
							"order": ["_json_"],
							"properties": {
								"_json_": {
									"title": "JSON",
									"type": "string",
									"je:hint": "codemirror",
									"je:codemirror": {
										"mode": "javascript/javascript",
										"path": "/_layouts/15/NVR.SPTools/libs/CodeMirror/",
										"addons": ["edit/matchbrackets"],
										"init": {
											"lineNumbers": true,
											"matchBrackets": true,
											"foldCode": true,
											"extraKeys": { "Enter": "newlineAndIndentContinueComment" }
										}
									}
								}
							}
						};
						if (typeof (jsoncontents) !== "string") {
							sch.properties["_json_"]["default"] = JSON.stringify(jsoncontents, null, 2);
						} else {
							sch.properties["_json_"]["default"] = jsoncontents;
						}

						return sch;
					};

					// ================= VYTVORENI EDITORU PRO SCHEMA a vysledneho formulare
					try {
						if (get_url_param("Default") == "") {
							try {
								window.JSONFields[fieldName].schema = loadFieldSchema(fieldName, jsonContentsParsed);
							} catch (le) {
								console.log("error loading field schema ", fieldName, le);
								window.JSONFields[fieldName].schema = window.JSONschemas.defaultSchema(jsonContents);
							}
						} else {
							window.JSONFields[fieldName].schema = window.JSONschemas.defaultSchema(jsonContentsParsed);
						}

						window.JSONFields[fieldName].schemaEditor = startEditor(fieldName, "JsonSchema Editor", "---");
					} catch (ee) {
						console.log("problem generating form from schema => using schemaless editor... with content:\n", jsonContents, "\nexception: ", ee);
					}
				});
		});
	};

	// funkce, kterou pri vykreslovani JSONFieldu bude poustet SharePoint,
	// ta vraci string s par <div>y, ktery ShP vepise do stranky, a 
	// navazuje callbacky
	function JSONFieldEditTemplate(ctx) {

		var formCtx = SPClientTemplates.Utility.GetFormContextForCurrentField(ctx);
		var n = formCtx.fieldName;
		var i = formCtx.fieldValue;
		var t = formCtx.fieldSchema.Title;

		if (!window.JSONFields) window.JSONFields = {};

		AddPostRenderCallback(ctx, function () {
			if (window.JSONFields[n] !== undefined) return;
			window.JSONFields[n] = {};
			window.JSONFields[n].originalValue = i;
			startJSON(n, i);
		});

		//console.log(cdm.getValue());

		// callback, kterym ShP ziska hodnotu pole pri ukladani formulare
		formCtx.registerGetValueCallback(n, function () {
			var form = window.JSONFields[n].form;
			if (!form) {
				console.error("json editor not active"); return "";
			}
			var result = form.collect(),
				errors;

			if ((result && result.ok) || (result.result && result.result.ok)) {
				errors = [];
			} else {
				errors = window.JSONFields[n].form.getErrors(result.result);
				console.error("json validation errors:\n", errors);
			}

			// pro pripad selhani schematu jsme pouzili Codemirror a hodnotu pole mame v klici _json_
			if (result.data["_json_"]) {
				if (typeof (result.data["_json_"]) == "string") return result.data["_json_"];
				else return JSON.stringify(result.data["_json_"], null, 2);
			}

			return JSON.stringify(result.data, null, 2);
		});
		
		// vracime HTML text k vykresleni do stranky
		return "<div id='editor" + n + "' style='width:960px;'></div>";

	};

	// registrace funkce JSONFieldEditTemplate do Sharepointu
	var jsonContext = {};

	// You can provide templates for:
	// View, DisplayForm, EditForm and NewForm
	jsonContext.Templates = {};
	jsonContext.Templates.Fields = {
		"JSONField": {
			// "View": JSONFieldViewTemplate,
			// "DisplayForm": JSONFieldViewTemplate,
			"EditForm": JSONFieldEditTemplate,
			"NewForm": JSONFieldEditTemplate
		}
	};

	SPClientTemplates.TemplateManager.RegisterTemplateOverrides(jsonContext);
})();

