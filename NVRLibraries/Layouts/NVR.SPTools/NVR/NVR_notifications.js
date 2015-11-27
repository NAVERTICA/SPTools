define(['jquery', 'underscore', 'nvr', 'nvr_general'/*,...*/], function ($, _, NVR/*,...*/) {
	"use strict";
	
	NVR.notifications = {
		closeStatusCallback: function(sid) {
			if (NVR.statusCloseCallbacks[sid]) NVR.statusCloseCallbacks[sid]();
		},

		showStatus: function(title, body, color, showClose, closeCallback, statusID) {
			if (!SP || !SP.UI || !SP.UI.Status) {
				setTimeout(function () {
				    NVR.notifications.showStatus(title, body, color, showClose, closeCallback, statusID);
				}, 1000);
				return;
			}

			// ukaze nejaky status message a vrati jeho id
			if (!window.openStatusIDs) window.openStatusIDs = {};
			if (window.openStatusIDs[statusID]) {
				SP.UI.Status.removeStatus(window.openStatusIDs[statusID]);
				window.openStatusIDs[statusID] = null;
			}
			var sid = SP.UI.Status.addStatus(title, body);

			if (color) {
				SP.UI.Status.setStatusPriColor(sid, color);
			}

			if (closeCallback) {
				NVR.statusCloseCallbacks[sid] = closeCallback; // pridat callback do globalni promenne
			}

			if (showClose) {
				SP.UI.Status.updateStatus(sid, "<a href='#' style='float:right;' onclick='javascript:SP.UI.Status.removeStatus(\"" + sid + "\"); closeStatusCallback(\"" + sid + "\"); return false;'>" + NVR.general.hlaska("closeStatus") + "</a>"
											   + body);
			}

			window.openStatusIDs[statusID] = sid;
		},

		removeStatusMessage: function(statusId) {
			if (!SP || !SP.UI || !SP.UI.Status) {
				setTimeout(function () {
				    NVR.notifications.removeStatusMessage(statusId);
				}, 5000);
				return;
			}
			if (window.openStatusIDs && window.openStatusIDs[statusId]) {
				SP.UI.Status.removeStatus(window.openStatusIDs[statusId]);
				window.openStatusIDs[statusId] = null;
			}
		},

		_openNotification: function(item, shownAlready) {
			var created = item["Created"];
			var title = item["Title"];
			var content = item["Content"] + " - " + created.toString("d") + " " + created.toString("t") + " " + item["Author"];

			var callback = function () {
				setCookie("notification" + item["ID"], "closed", 365, "/");
				setCookie("notification_open" + item["ID"], null); // zrusime informaci o tom, ze je notifikace aktualne otevrena
			};

			var hranice = new Date();
			hranice = new Date(hranice.setDate(hranice.getMinutes() + 5));

			setCookie("notification_open" + item["ID"], "open", hranice); // nezapiseme domenu, takze "open" nebude to platit na zadne jine strance

			this.showStatus(title, content, item["Color"], true, callback);
		},

		runNotifications: function(showAll) {
			return;
			// podle konfigurace seznamu ve web.config začne na pozadí kontrolovat, jestli se nevyskytly nové notifikace, a případně je v jGrowl zobrazí
			$.ajaxSetup({ cache: false });
			$.ajax({
				cache: false,
				dataType: "text",  // kdybychom pouzili jquery json parser, nezvladne sam nacist datumy
				url: "/_layouts/15/SPTools/SiteNotifications.aspx",
				success: function (dataRaw) {
					window.top.console.group("SiteNotifications");
					var data = [];
					try {
						data = eval(dataRaw);
					} catch (exc) {
						window.top.console.dir(exc);
						window.top.console.info(dataRaw);
					}

					try {
						if (data.length > 0) {
							window.top.console.info("%d loaded notifications", data.length);
							$("#notificationsTD").show();
						} else if (showAll) {
							showStatus(hlaska("noActiveNotifications", "", "blue", true));
						}

						$.each(data, function (i, item) {
							var closedAlready = getCookie("notification" + item["ID"]);
							var currentlyOpen = getCookie("notification_open" + item["ID"]);
							window.top.console.info("notification %d - closedAlready == %s, currentlyOpen == %s", item["ID"], closedAlready, currentlyOpen);
							// pokud mame notification_open cookie, je notifikace na teto strance aktualne otevrena a otevirat ji znovu nebudeme
							// pokud mame showAll, otevirame vsechny co ted otevrene nejsou, kdyz ne, otevreme jen v pripade, ze nemame closedAlready
							if (!currentlyOpen && (showAll || !closedAlready)) {
								_openNotification(item);
								navert_notif[item["Created"]] = true;
							}
						});
					} catch (exc) {
						reportError(exc, false);
					}
					window.top.console.groupEnd();
				} /*,
				error: function (x, y, z) {
					window.top.console.group("SiteNotifications load error");
					window.top.console.error(x, y, z);
					window.top.console.groupEnd();
				}*/
			});
		}
	};
	
	return NVR;
});