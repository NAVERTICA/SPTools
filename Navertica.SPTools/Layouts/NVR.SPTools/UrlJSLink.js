//URL jsLink

(function () {
    function UrlField(name, value) {
        window.stfu = "stfu";
        //load textarea value here, was not loading otherwise
        document.getElementById("urlcm").value = value;
        var getExecutorCallPromise = function (url, executor) {
            return new Promise(function (resolve, reject) {
                executor.executeAsync({
                    url: url,
                    method: "GET",
                    headers: { "Accept": "application/json;odata=verbose" },
                    success: function (data) {
                        resolve(data);
                    },
                    error: function (data, errCode, ErrMess) {
                        reject(data, ErrMess);
                    }
                });
            });
        };

        var loadUrlField = function (Q) {
            var host = window.location.protocol + '//' + window.location.host;
            var executor = new SP.RequestExecutor(host);
            var webOptions = [];
            var listOptions = [];
            window.urlfield = getExecutorCallPromise(host + "/_api/web", executor)
				.then(function (data) {
				    /*...*/
				    var result = JSON.parse(data.body).d;
				    webOptions.push({ "listsUrl": result.Lists.__deferred.uri, "title": result.Title || result.Url, "relUrl": result.ServerRelativeUrl });
				    /*...*/
				    return getExecutorCallPromise(host + "/_api/web/webs", executor)
				})
				.then(function (data) {
				    /*...*/
				    var results = JSON.parse(data.body).d.results;
				    $.each(results, function () {
				        webOptions.push({ "listsUrl": this.Lists.__deferred.uri, "title": this.Title || this.Url, "relUrl": this.ServerRelativeUrl });
				    });

				    var allListUrls = [];

				    $.each(webOptions, function () {
				        allListUrls.push(this.listsUrl);
				        var option = document.createElement("option");
				        option.value = this.listsUrl;
				        option.title = this.relUrl;
				        option.innerHTML = this.title;
				        $('#uWeb')[0].appendChild(option);
				    });

				    /*...*/
				    return Q.all(allListUrls.map(function (el, i) {
				        return getExecutorCallPromise(el, executor)
							.then(function (data) {
							    var results = JSON.parse(data.body).d.results;
							    $.each(results, function () {
							        listOptions.push({ "fieldsUrl": this.Fields.__deferred.uri, "title": this.Title || this.Id, "baseTemplate": this.BaseTemplate, "entity": this.EntityTypeName.indexOf("List") > 0 ? ("Lists/" + this.EntityTypeName.replace("List", "")) : this.EntityTypeName });
							    });
							});
				    }));
				})
				.then(function (data) {
				    var appendOption = function (opt, id) {
				        var option = document.createElement("option");
				        option.value = opt.fieldsUrl;
				        option.innerHTML = opt.title;
				        option.category = opt.baseTemplate;
				        option.title = opt.entity;
				        $('#uList optgroup[id="' + id + '"]')[0].appendChild(option);
				    };
				    var appendOptGroup = function (opt) {
				        var optgroup = document.createElement("optgroup");
				        optgroup.id = opt.baseTemplate;
				        optgroup.label = "Base Template: " + opt.baseTemplate;
				        $('#uList')[0].appendChild(optgroup);
				    };
				    //sort options based on base template
				    listOptions.sort(function (a, b) {
				        var keyA = parseInt(a.baseTemplate);
				        var keyB = parseInt(b.baseTemplate);
				        if (keyA < keyB) return -1;
				        if (keyA > keyB) return 1;
				        return 0;
				    });
				    var tempOptGroupId = "";
				    for (var i = 0; i < listOptions.length; i++) {
				        if (i == 0) {
				            appendOptGroup(listOptions[i]);
				            tempOptGroupId = listOptions[i].baseTemplate;
				        } else if (parseInt(listOptions[i - 1].baseTemplate) != parseInt(listOptions[i].baseTemplate)) {
				            appendOptGroup(listOptions[i]);
				            tempOptGroupId = listOptions[i].baseTemplate;
				        }
				        appendOption(listOptions[i], tempOptGroupId);
				    }
				})
				.then(function (data) {
				    $('#uWeb')[0].onchange = function () {
				        var selection = $('#uWeb option:selected');
				        selection.each(function (i, el) {
				            var elementTitle = this.title;
				            if (elementTitle == "/") elementTitle = host;
				            $('#uList').children().children().each(function (i, el) {
				                if (this.value.indexOf(elementTitle + "/_api/Web") == -1) {
				                    this.style.display = "none";
				                    this.disabled = "disabled";
				                    if (this.selected == true) this.selected = false;
				                } else {
				                    this.style.display = "";
				                    this.disabled = "";
				                }
				            });
				        });
				        //disable empty optGroups
				        $('#uList').children().each(function () {
				            if ($(this).find("option[disabled='']").length == $(this).children().length) {
				                this.style.display = "none";
				                this.disabled = "disabled";
				            } else {
				                this.style.display = "";
				                this.disabled = "";
				            }
				        });
				    };
				    $('#uWeb')[0].onchange();

				    var onUButtonClick = function () {
				        var url = ($('#uWeb option:selected')[0].title == '/' ? '' : $('#uWeb option:selected')[0].title) + '/' + $('#uList option:selected')[0].title + '/' + $('#uForm option:selected')[0].value;
				        if ($('#urlcm')[0].value == "") {
				            $('#urlcm')[0].value += url;
				        } else {
				            $('#urlcm')[0].value += '|' + url;
				        }
				    };

				    $('#uButton')[0].onclick = onUButtonClick;
				    console.log("UrlField Loaded.");
				    return "UrlField Loaded";
				})
				.catch(function (err) {
				    console.log(err);
				});
        }
		
		RegisterSod("sp.requestexecutor.js", "/_layouts/15/SP.RequestExecutor.js");
		RegisterSod("require.js", "/_layouts/15/NVR.SPTools/libs/requirejs/require.js");
		RegisterSod("jquery.js", "/_layouts/15/NVR.SPTools/libs/jquery-ui/external/jquery-1.9.1/jquery.js");
		RegisterSodDep('require.js', 'jquery.js');
		RegisterSodDep('jquery.js', 'sp.requestexecutor.js');
		SP.SOD.executeFunc("require.js", null ,function(){
			require.config({
				paths: {
					"Q": "/_layouts/15/NVR.SPTools/libs/q/q"
				}
			});

			require(["Q"], function (Q) {
				//window.Q = require("Q");
				loadUrlField(Q);
			});
		});
    };

    function editTemplate(ctx) {
        var formCtx = SPClientTemplates.Utility.GetFormContextForCurrentField(ctx);
        var n = formCtx.fieldName;
        var i = formCtx.fieldValue;
        var t = formCtx.fieldSchema.Title;
        AddPostRenderCallback(ctx, function () {
            if (window.stfu == "stfu") return;
            UrlField(n, i);
        });

        formCtx.registerGetValueCallback(n, function () {
            var newValue = document.getElementById('urlcm').value;
            //newValue = newValue.replace(/\n/g,"\u21b5");
            //newValue = newValue.replace(/\n/g,"↵");
            //debugger;
            //alert(newValue);
            if (!newValue) { return ' '; }
            //formCtx.fieldValue = newValue;
            return newValue;
        });
        return "<div id=UrlField" + n + ">\n" +
				"<span>Web: </span><select id=\"uWeb\"></select>" +
				"<span>List: </span><select id=\"uList\"></select>" +
				"<span>For: </span><select id=\"uForm\">" +
				"	<option value=\"*\">All</option>" +
				"	<option value=\"*Form.aspx\">All Forms</option>" +
				"	<option value=\"*EditForm.aspx\">Edit Form</option>" +
				"	<option value=\"*DispForm.aspx\">Display Form</option>" +
				"</select>" +
				"<input type=\"button\" id=\"uButton\" value=\"Add\">" +
				"</br>" +
				"<textarea rows=\"6\" cols=\"20\" id=\"urlcm\" title=\"urlcm\" class=\"ms-long\" ></textarea>" + //value=\""+i+"\"
				"</div>";
    };

    var context = {};

    // You can provide templates for:
    // View, DisplayForm, EditForm and NewForm
    context.Templates = {};
    context.Templates.Fields = {
        "NVR_SiteConfigUrl": {
            // "View": JSONFieldViewTemplate,
            // "DisplayForm": JSONFieldViewTemplate,
            "EditForm": editTemplate,
            "NewForm": editTemplate
        }
    };
    //context.OnPostRender = loadRest;
    SPClientTemplates.TemplateManager.RegisterTemplateOverrides(context);
})();

