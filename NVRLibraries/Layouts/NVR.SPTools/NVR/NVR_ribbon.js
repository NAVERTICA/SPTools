define(['jquery', 'underscore', 'nvr', 'nvr_general', 'nvr_e_n_d_form', 'nvr_form'/*,...*/], function ($, _, NVR/*,...*/) {
	"use strict";
	
	NVR.ribbon = {
		EnableButtonOne: function() {
			var ctx = SP.ClientContext.get_current();
			var items = SP.ListOperation.Selection.getSelectedItems(ctx);
			return items.length == 1;
		},

		EnableButtonMoreThenOne: function() {
			var ctx = SP.ClientContext.get_current();
			var items = SP.ListOperation.Selection.getSelectedItems(ctx);
			return items.length > 0;
		},

		GetSelectedItemsId: function() {
			var ctx = SP.ClientContext.get_current();
			var items = SP.ListOperation.Selection.getSelectedItems(ctx);
			var selectedItems = [];
			for (index in items) {
				selectedItems[index] = items[index].id;
			}
			return selectedItems;
		},

		//depends on ContextGuids
		GetItemsData: function(itemIds, query) {
			//funguje zatim jen pro jednu polozku //TODO - udelat tak aby umela servica vracet i pro vice polozek
			if(query == undefined)
			{
				query = "";
			}

			var result;
			//$select=Author/Id,Author/Title,Editor/Id,Editor/Title,*&$expand=Author,Editor - vicenasobny vyber pro lokuupove polozky
			var url = ContextGuids.siteurl + ContextGuids.weburl + "/_api/web/lists(guid'" +  ContextGuids.list + "')/items(" +  itemIds + ")?" + query;

			$.ajax(
			{
				method: "GET",
				headers: { "Accept": "application/json; odata=verbose" },
				"async": false,
				"url": url,
				"success": function (data)
				{
					result = data.d;
				},
				"error": function (XMLHttpRequest, textStatus, errorThrown)
				{
					//alert(XMLHttpRequest.responseText);
					result = null;
				}
			});

			return result;
		},

		//depends on ContextGuids
		GetDefaultFormUrl: function(itemId, dispform) {
			var formUrl = "";

			if(dispform == undefined || dispform )
			{
				dispform = true;
				formUrl = "DefaultDisplayFormUrl";
			}
			else
			{
				formUrl = "DefaultEditFormUrl";
			}

			var url = ContextGuids.siteurl + ContextGuids.weburl + "/_api/web/lists(guid'" +  ContextGuids.list + "')/" + formUrl;

			$.ajax(
			{
				method: "GET",
				headers: { "Accept": "application/json; odata=verbose" },
				"async": false,
				"url": url,
				"success": function (data)
				{
					if(dispform == undefined || dispform )
					{
						formUrl = ContextGuids.siteurl + data.d.DefaultDisplayFormUrl + "?ID=" + itemId;
					}
					else
					{
						formUrl = ContextGuids.siteurl + data.d.DefaultEditFormUrl + "?ID=" + itemId;
					}
				},
				"error": function (XMLHttpRequest, textStatus, errorThrown)
				{
					//alert(XMLHttpRequest.responseText);
					formUrl = null;
				}
			});
			return formUrl;
		},

		DisableRibbonOnNewFormOrNotSelectedInView: function() {

			if (NVR.general.isFormPage()) {
				if (NVR.general.isNewFormPage()) {
					return false;
				}
				else {
					return true;
				}
			}
			else {
				return this.EnableButtonMoreThenOne();
			}
		},

		DisableRibbonOnNewFormOrNotOnlyOneSelectedInView: function() {
			if (NVR.general.isFormPage()) {
				if (NVR.general.isNewFormPage()) {
					return false;
				}
				else {
					return true;
				}
			}
			else {
				return this.EnableButtonOne();
			}
		},

		DisableRibbonOnNewForm: function() {
			if (NVR.general.isNewFormPage()) return false;
			return true;
		},

		//depends on ContextGuids
		ClipboardCopyUrl: function(docurl, listId) {
			$.clipboardReady(function () {
				if (NVR.general.isFormPage() && !(ContextGuids.documentUrl == undefined || ContextGuids.dispFormUrl == undefined)) {
					if (docurl) {
						$.clipboard(ContextGuids.documentUrldocumentUrl);
					}
					else {
						$.clipboard(ContextGuids.dispFormUrl);
					}
				}
				else {
					$.ajax(
						{
							"url": "/_layouts/15/CScript.aspx?ConfigName=GetUrlsService&app=Template&wli=" + ContextGuids.web + ":" + listId + "&ids=" + GetSelectedItemsId().join() + "&docurl=" + docurl,
							"success": function (data, textStatus) {
								$.clipboard(data);
							},
							"error": function (XMLHttpRequest, textStatus, errorThrown) {
								alert(XMLHttpRequest.responseText);
							},
							"dataType": "text"
						});
				}
			});
		},

		//depends on ContextGuids
		CreateAttachmentToLibrary: function(listId) {

			var listGuid = ContextGuids.item != null ? ContextGuids.list : SP.ListOperation.Selection.getSelectedList();
			listGuid = listGuid.toLowerCase().replace("{", "").replace("}", "");

			var id;

			if (NVR.general.isFormPage())
			{
				id = ContextGuids.item.ID;
			}
			else
			{
				id = this.GetSelectedItemsId()[0];
			}

			var item = this.GetItemsData(id, "$select=Title,FieldValuesAsText/FileRef&$expand=FieldValuesAsText");
			var p = item.FieldValuesAsText.FileRef.split('/');

			var attachedLinks = this.GetDefaultFormUrl(id) + "," + p[p.length - 1];
			var foldername = listGuid + "/" + item.ID;
			var createFolderUrl =  ContextGuids.siteurl + ContextGuids.weburl + "/_layouts/15/CScript.aspx?ConfigName=CreateFolder&app=Template&list=" + ContextGuids.documentsAttachmentsId + "&FolderName=" + foldername;

			$.ajax(
			{
				"dataType": "text",
				"type": "POST",
				"url": createFolderUrl,
				"success": function (data) {
					var source = encodeURIComponent(window.top.location);
					var uploadUrl = ContextGuids.siteurl + ContextGuids.weburl + "/_layouts/15/Upload.aspx?List=" + ContextGuids.documentsAttachmentsId + "&NVR_AttachedLinks=" + encodeURIComponent(attachedLinks) + "&RootFolder=NVR_Attachments/" + foldername + "&Source=" + source;

					//window.top.location = uploadUrl;
					//window.top.location.reload(true);
					openDialog(uploadUrl, function () { window.top.location.reload(true); });
				},
				"error": function (XMLHttpRequest, textStatus, errorThrown) {
					alert(XMLHttpRequest.responseText);
				}
			});
		},

		newFormCreateTaskForItem: function() {
			var title;
			var item;
			if (ContextGuids.item != null)
			{
				item = ContextGuids.item;
			}
			else
			{
				item = this.GetItemsData(this.GetSelectedItemsId()[0]);
			}

			title = item.Title;
			if(title == null)
			{
				try
				{
					//toto by melo nastat jen u knihovny
					title = item.FileLeafRef;
				}
				catch (e)
				{
					window.top.console.log("Missing Title or name of the item");
				}
			}

			var itemUrl = ContextGuids.listUrl + "DispForm.aspx?ID=" + item.ID + "," + title;
			var taskFormUrl = ContextGuids.siteurl + "/cfp/Lists/Tasks/NewForm.aspx?&NVR_ParentDocument=" + encodeURIComponent(itemUrl);
			window.top.location = taskFormUrl;
			//openDialog(newFormUrl, function() { window.top.location.reload(true); });
			return false;
		},

		PoradyRibbonsEnabled: function() {
			return ContextGuids.meetingRibbonEnabled === "true";
		},

		PoradySendEmailInvitation: function(listId, itemID) {
			var viewLink = "/_layouts/15/SPTools/EmailInvite.aspx?WebID=" + ContextGuids.web + "&List=" + listId + "&ID=" + itemID;
			NVR.form.openDialog(viewLink);
		},

		MultiEditContentType: function() {
			var body = $('<div><select id=\'selId000\'></select><button onClick=\'MultiEditContentTypeServiceCall()\';>Save</button></div>');

			$.each(ContextGuids.contentTypes, function (key, value) {
				var opt = document.createElement('option');
				opt.value = key;
				opt.innerHTML = value;
				body.find("select")[0].appendChild(opt);
			});

			var options = {
				title: " ",
				width: 200,
				height: 100,
				html: body[0]
			};

			SP.UI.ModalDialog.showModalDialog(options);
		},

		MultiEditContentTypeServiceCall: function() {
			var ct = $('#selId000').find(":selected")[0].value;

			//ConfigName=MultiEditContentType&app=Template&Web=/&List=JFL&Ids=48,49&CT=0x010003DDC76FD2FB69409B972E0B29BB02A2
			var url = ContextGuids.siteurl + ContextGuids.weburl + "/_layouts/15/CScript.aspx?ConfigName=MultiEditContentType&app=Template&List=" + SP.ListOperation.Selection.getSelectedList() + "&ids=" + this.GetSelectedItemsId().join(",") + "&ct=" + ct;

			$.ajax(
			{
				"dataType": "text",
				"type": "POST",
				"url": url,
				"success": function (data) {
					window.top.location.reload(true);
				},
				"error": function (XMLHttpRequest, textStatus, errorThrown) {
					alert(XMLHttpRequest.responseText);
				}
			});

			SP.UI.ModalDialog.commonModalDialogClose();
		},

		PoradyGenerateReport: function(listId, itemID) {
			var viewLink = "/_layouts/15/SPTools/GenerateReport.aspx?Id=" + itemID + "&List=" + ContextGuids.list + "&WebId=" + ContextGuids.web /*+ "&IsDlg=1" */ + "&Source=" + GetSource();
			SP.UI.ModalDialog.showWaitScreenWithNoClose("Prosíme vyčkejte", "Probíhá vytváření zápisu z porady - může to trvat i delší dobu", 150, 500);
			window.location.href = viewLink;
			//openDialog(viewLink);
		},

		NormalizeLoginName: function(login){
			return login.replace("i:0#.w|", "").replace(/\\/g, "\\\\");
		},

		ExtendedTaskRibbonManipulate_Enabled: function(action){
			var id;
			if(NVR.general.isFormPage())
			{
				if (NVR.general.isNewFormPage()) return false;
				id = ContextGuids.item.ID;
			}
			else
			{
				//Zatim nechame povolen pouze jednu polozku
				if(!this.EnableButtonOne()) return false;
				id = this.GetSelectedItemsId()[0];
			}

			var item = this.GetItemsData(id, "$select=Status,ContentTypeId,NVR_TaskFinished,DueDate,AssignedTo/Id&$expand=AssignedTo");

			var moveCtId = "0x01080000E87E2DED007A54000000000000DDDD";
			if(item.ContentTypeId.indexOf(moveCtId) === 0 )//Posouvaci ukol
			{
				if(action == "Approve")	return true;
				if(action == "Reject") return true;
			}

			if (item.NVR_TaskFinished) return false;

			//TODO - dalsi jazyky
			var status_NotStarted = ["NotStarted", "Nezahájeno"];
			var status_InProgress = ["InProgress", "Probíhá"];
			var status_Completed = ["Completed", "Dokončeno"];
			var status_Deferred = ["Deferred", "Odloženo"];
			var status_Waiting = ["Waiting on someone else", "Čeká se na někoho dalšího"];

			if(action == "Acceptance")//Přijmout úkol
			{
				if (status_NotStarted.indexOf(item.Status) > -1) return true;
			}

			if(action == "Refuse") // Odmítnutout úkol
			{
				if (status_NotStarted.indexOf(item.Status) > -1) return true;
			}

			if(action == "Cancel") // Zrušit úkol
			{
				if (status_NotStarted.indexOf(item.Status) > -1) return true;
				if (status_Deferred.indexOf(item.Status) > -1) return true;
			}

			if(action == "Complete") //Dokončit úkol
			{
				if (status_InProgress.indexOf(item.Status) > -1) return true;
			}

			if(action == "Approve") //Schválit úkol
			{
				if (status_Completed.indexOf(item.Status) > -1) return true;
			}

			if(action == "Reject") //Zamítnout úkol
			{
				if (status_Completed.indexOf(item.Status) > -1) return true;
			}

			if(action == "MoveTask") //Posun termínu
			{
				if (status_Deferred.indexOf(item.Status) > -1) return false;
				if (status_Completed.indexOf(item.Status) > -1) return false;

				var enabled = false;

				//TODO - udelat jako servicu do siteconfigu nez to mit jako script primo v konfiguraci
				$.ajax({
					"async": false,
					"cache": false,
					"url": ContextGuids.weburl + "/_layouts/15/CScript.aspx?Web=" + ContextGuids.web + "&App=ExtendedTaskReceiver" + "&ConfigName=" + ContextGuids.listDefaultViewUrlNoASPX + "&Move=1&AssignedTo=" + item.AssignedTo.results[0].Id,
					"success": function (data, textStatus)
					{
						enabled = data.toLowerCase() === "true";
					},
					"error": function (XMLHttpRequest, textStatus, errorThrown)
					{
						alert(XMLHttpRequest.responseText);
						//alert(Hlasky["Error"]);
						//window.location.reload(true);
						enabled = false;
					},
					"dataType": "text"
				});
				//alert("EnableMoveExtendedTask - " + enabled);
				return enabled;
			}

			if(action == "Divide")	//Rozdělení
			{
				if (status_Deferred.indexOf(item.Status) > -1) return false;
				if (status_Completed.indexOf(item.Status) > -1) return false;

				var currentDate = new Date();
				var dueDate = new Date(item.DueDate);
				var dateResult = dueDate >= currentDate;

				if(!dateResult)
				{
					window.top.console.log("EnableDivideExtendedTask - FALSE:" + dueDate + " > " + currentDate);
					return false;
				}
				var rights = null;

				$.ajax({
					"async": false,
					"cache": false,
					"url": ContextGuids.weburl + "/_layouts/15/CScript.aspx?Web=" + ContextGuids.web + "&App=ExtendedTaskReceiver" + "&ConfigName=" + ContextGuids.listDefaultViewUrlNoASPX,
					"success": function (data, textStatus)
					{
						//alert("EnableDivideExtendedTask - " + data);
						rights = data.toLowerCase() === "true";
						window.top.console.log("EnableDivideExtendedTask RIGHTS : '" + data + "'");
					},
					"error": function (XMLHttpRequest, textStatus, errorThrown)
					{
						alert(XMLHttpRequest.responseText);
						//alert(Hlasky["Error"]);
						//window.location.reload(true);
						return false;
					},
					"dataType": "text"
				});

				return rights ;
			}

			return false;
		},

		ExtendedTaskRibbonManipulate: function(action){
			if(action == "Cancel")
			{
				var messages = {};
				messages["cs"] = "Opravdu chcete zrušit úkol?";
				messages["en"] = "Do you really want to cancel the task?";
				messages["sk"] = "Naozaj chcete zrušiť úlohu?";
				messages["ru"] = "Вы действительно хотите отменить задание?";

				var r = confirm(messages[ContextGuids.userlangiso]);
				if (r == false)
				{
					return;
				}
			}

			var id;
			if(NVR.general.isFormPage())
			{
				id = ContextGuids.item.ID;
			}
			else
			{
				id = this.GetSelectedItemsId()[0];
			}

			var item = this.GetItemsData(id, "$select=Id,Title,Body,Predecessors,StartDate,DueDate,NVR_Output,NVR_Company,NVR_FYI/Id,NVR_FYI/Name,NVR_ParentTopic/Id,Predecessors/Id,NVR_Issuer/Id,NVR_Issuer/Name,AssignedTo/Id,AssignedTo/Name&$expand=AssignedTo,NVR_Issuer,Predecessors,NVR_ParentTopic,NVR_FYI");

			if(action == "Divide" || action == "MoveTask")
			{
				var assignedTo = this.NormalizeLoginName(item.AssignedTo.results[0].Name);
				var issuer = this.NormalizeLoginName(item.NVR_Issuer.Name);
				//Musi byt single lookup
				var parentTopic = item.NVR_ParentTopic.Id != undefined ? item.NVR_ParentTopic.Id : "";

				if(action == "Divide")
				{
					//var predecessors = item.Predecessors.Id != undefined ? item.Predecessors.Id : "";

					var today = new Date();
					var startDate = new Date(item.StartDate);
					if(startDate < today)
					{
						startDate = today;
					}

					var body = item.Body;
					if(body == null) body = "";

					var FYI = "";
					if(item.NVR_FYI.results != undefined )
					{
						for(var i=0; i < item.NVR_FYI.results.length; i++ )
						{
							FYI += this.NormalizeLoginName(item.NVR_FYI.results[i].Name);

							if(i< item.NVR_FYI.results.length) FYI += ",";
						}
					}

					var formUrl = ContextGuids.listUrl + "NewForm.aspx?ContentTypeId=" + ContextGuids.extendedTaskLisCt;
					var parameters = "&Predecessors=" + item.Id + "&NVR_Issuer=" + assignedTo + "&StartDate=" + startDate.toISOString() + "&DueDate=" + item.DueDate + "&NVR_ParentTopic=" + parentTopic + "&NVR_Company=" + encodeURIComponent(item.NVR_Company) + "&Body=" + encodeURIComponent(body) + "&NVR_FYI=" + FYI;
					window.top.location = formUrl + parameters;

					//var newFormUrl =  + "/_layouts/15/SPTools/NewFormContentType.aspx?ContentTypeId=0x01080000E87E2DED007A540000000000000000&FromID=" + Url.encode(window.location.href);
					//openDialog(newFormUrl, function () { window.top.location.reload(true); });
					return;
				}

				if(action == "MoveTask")
				{
					var title = {};
					title["cs"] = "Žádost o posunutí termínu ukončení pro úkol";
					title["en"] = "Task Due Date change request";
					title["sk"] = "Žiadosť o posunutie termínu ukončenia pre úlohu";
					title["ru"] = "Просьба об отсрочке выполнения задания";

					var body = {};
					body["cs"] = "Žádám o posunutí termínu ukončení pro úkol \"#Task#\", z důvodu ...";
					body["en"] = "I am requesting a Due Date change for the task \"#Task#\", because...";
					body["sk"] = "Žiadam o posunutie termínu ukončenia pre úlohu \"#Task#\", z dôvodu ...";
					body["ru"] = "Прошу перенести срок для выполнения задания \"#Task#\", потому что ...";

					// encodeURIComponent(ContextGuids.extendedTaskMoveDueDateTitle) - TODO DODELAT PREKLADY
					title = encodeURIComponent(title[ContextGuids.userlangiso] + " - " + item.Title);
					body = body[ContextGuids.userlangiso].replace("#Task#", item.Title);
					var today = new Date();
					var startDate = new Date(item.StartDate);
					if(startDate < today)
					{
						startDate = today;
					}
					var dueDate = new Date(today.setDate(today.getDate() + 3)).toISOString();

					//replace(/\\/g, "\\\\")
					var formUrl = ContextGuids.listUrl + "NewForm.aspx?ContentTypeId=" + ContextGuids.extendedTaskMoveDueDateLisCt;
					var parameters = "&Predecessors=" + item.Id + "&NVR_Issuer=" + assignedTo+ "&AssignedTo=" + issuer + "&StartDate=" + startDate.toISOString() + "&DueDate=" + dueDate + "&Title=" + title + "&Body=" + encodeURIComponent(body) + "&NVR_Company=" + encodeURIComponent(item.NVR_Company);
					window.top.location = formUrl + parameters;

					// var newFormUrl = webUrl + "/_layouts/15/SPTools/NewFormContentType.aspx?ContentTypeId=0x01080000E87E2DED007A54000000000000000000FF0000302ED2EDA7E00004E5294F0001&FromID=" + Url.encode(window.location.href) + "&MoveDueDate=true";
					// openDialog(newFormUrl, function () { window.top.location.reload(true); });
					return;
				}
			}

			SP.UI.ModalDialog.showWaitScreenWithNoClose('', '', 75, 75);

			//Zbytek actions jede pres C# script
			if(action == "Refuse" || action == "Cancel" || action == "Reject" )
			{
				var messages = {};
				messages["cs"] = "Nezadal jste důvod do pole Řešení – chcete přesto pokračovat?";
				messages["en"] = "You have not entered any reason into „Solution“ field – do you really want to continue?";
				messages["sk"] = "Nezadali ste dôvod do pola „Riešenie“. Chcete napriek tomu pokračovať?";
				messages["ru"] = "Není vyplněno pole Řešení. Chcete pokračovat?";

				if(NVR.general.isEditFormPage())
				{
					var outputForm = getFormBodyText("NVR_Output");
					var outputItem = item["NVR_Output"] || "";

					if(outputForm == outputItem)
					{
						var r = confirm(messages[ContextGuids.userlangiso]);
						if (r == false)
						{
							SP.UI.ModalDialog.commonModalDialogClose(1, '');//loading screen close
							return;
						}
					}
				}
				else //pokud nejsme na editformu tak redirectujeme
				{
					var r = confirm(messages[ContextGuids.userlangiso]);
					if (r == false)
					{
						var url =  this.GetDefaultFormUrl(item.Id, false);

						window.location = url;
						return;
					}
				}
			}

			var metadata = "&metadata=0";
			if (NVR.general.isEditFormPage())
			{
				var output = encodeURIComponent(NVR.e_n_d_form.getFormBodyText("NVR_Output"));
				var title = encodeURIComponent(NVR.e_n_d_form.getFormBodyText("Title"));
				//var priority = encodeURIComponent(getFormBodyText("Priority"));

				var body = $(getFormBodyHtml("Body")).find("div");
				if(body.length > 0)
				{
					//var body = encodeURIComponent(getFormBodyText("Body")); //vraci prazdne <p> </p>
					body = encodeURIComponent(body[2].innerHTML);
				}
				else
				{
					body = "";
				}

				var fyi = NVR.e_n_d_form.getFormBodyText("NVR_FYI");
				if(fyi != "")
				{
					var f = JSON.parse(fyi);
					fyi = "";

					for(var i=0; i < f.length; i++ )
					{
						fyi += NormalizeLoginName(f[i].Key);
						if(i < f.length) fyi += ",";
					}
				}
				metadata = "&metadata=1&Title=" + title  + "&Body=" + body + "&NVR_Output=" + output + /*"&Priority=" + priority + */ "&NVR_FYI=" + fyi;
			}

			var wli = ContextGuids.web + ":" + ContextGuids.list + ":" + item.Id;
			var url = "/_layouts/15/CScript.aspx?ConfigName=ExtendedTaskRibbonManipulate&app=Template&Wli=" + wli + "&Action=" + action + metadata;

			$.ajax({
				"async": false,
				"cache": false,
				"url": url,
				"success": function (data, textStatus)
				{
					window.location = this.GetDefaultFormUrl(item.Id);
					window.top.console.log("ExtendedTaskRibbonManipulate - " + action + " : '" + data + "'");
					SP.UI.ModalDialog.commonModalDialogClose(true);
				},
				"error": function (XMLHttpRequest, textStatus, errorThrown)
				{
					alert(XMLHttpRequest.responseText);
					//alert(Hlasky["Error"]);
					//window.location.reload(true);
					SP.UI.ModalDialog.commonModalDialogClose(true);
				},
				"dataType": "text"
			});

			SP.UI.ModalDialog.commonModalDialogClose(1, '');//loading screen close
		}
	};
	
	return NVR;
});