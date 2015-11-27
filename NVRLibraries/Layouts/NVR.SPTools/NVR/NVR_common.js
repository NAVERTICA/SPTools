define(['jquery', 'underscore', 'nvr'/*,...*/], function ($, _, NVR/*,...*/) {
	"use strict";
	
	NVR.common = {
		ensureConsole: function() { //
			// moznosti logovani do konzole - http://getfirebug.com/logging
			// je lepsi volat konzoli pomoci window.top.console, pak funguje spolehlive i z iframe apod.
			if (typeOf(window.top.console) == "undefined") {
				window.top.console = {
					log: function (text) { }, error: function (text) { }, warn: function (xx) { }, info: function (x) { },
					group: function (grp) { }, groupEnd: function (xxx) { }, dir: function (x) { }, debug: function (xx) { },
					trace: function () { }, time: function () { }, timeEnd: function () { }, profile: function () { }, profileEnd: function () { },
					assert: function (x) { }, dummy: true
				}
				// IE console nektere funkce postrada
			} else if (typeOf(window.top.console.group) == "undefined") {
				window.top.console["group"] = function (x) { };
				window.top.console["groupEnd"] = function (x) { };
				window.top.console["dir"] = function (x) { };
				window.top.console["time"] = function (x) { };
				window.top.console["timeEnd"] = function (x) { };
				window.top.console["profile"] = function (x) { };
				window.top.console["profileEnd"] = function (x) { };
				window.top.console["assert"] = function (x) { };
			}

			if (!window.top.console.error) {
				window.top.console.error = window.top.console.log;
			}

			if (!window.top.console.warn) {
				window.top.console.warn = window.top.console.log;
			}

			if (!window.top.console.info) {
				window.top.console.info = window.top.console.log;
			}
		},
		
		isdefined: function(variable) { //
			try {
				return (typeOf(window[variable]) == "undefined") ? false : true;
			} catch (e) {
				return false;
			}
		},
		
		typeOf: function(value) { // co treba rozsirit o isEmptyObject a jine?
			var s = typeof value;
			if (s === 'object') {
				if (value) {
					if (value instanceof Array) {
						s = 'array';
					}
				} else {
					s = 'null';
				}
			}
			return s;
		},
		
		getServerRelativeUrl: function(url) { //
			var result = url.substring(7);

			return result.substring(result.indexOf('/')).split("?")[0];
		},
		
		nbspReplace: function(text) { // nahrazení html non-breaking spaces (&nbsp;), zakodovanych v url, na bezne mezery
			return (text + "").replace(unescape("%A0"), " ");
		},
		
		// pokud je seznam seskupeny, tzn. ma rozklikavaci slozky, tohle zajisti spusteni dodane funkce po otevreni slozky
		remapFolderRendering: function(func) {// {{{
			var ExpGroupRenderData2 = ExpGroupRenderData;

			ExpGroupRenderData = function (torender, groupName, isLoaded) {
				ExpGroupRenderData2(torender, groupName, isLoaded);

				if (g_ExpGroupInProgress && isLoaded) {
					func();
				}
			};
		},
		
		html_entity_decode: function(str) {// {{{
			var ta = document.createElement("textarea");
			ta.innerHTML = str.replace(/</g, "&lt;").replace(/>/g, "&gt;");
			return ta.value;
		},
		
		//instead jQuery.inArray( value, array [, fromIndex ] )	https://api.jquery.com/jQuery.inArray/
		/*
		arrayIndexOf: function(needle, hay) { // {{{ // hledani v poli, IE nepodporuje
			for (var i = 0; i < hay.length; i++)
				if (hay[i] == needle) {
					return i;
				}
			return -1;
		},
		*/
		containedIn: function(needle, hay) { // {{{
			if (typeOf(needle) != "array") {
				needle = [needle];
			}

			if (typeOf(hay) != "array") {
				hay = [hay];
			}

			return _(needle).any(function (i) {
				return _(hay).any(function (w) {
					return w.indexOf(i) != -1;
				});
			});

		},
		
		query_string: function () {
		  var query_string = {};
		  var query = window.location.search.substring(1);
		  var vars = query.split("&");
		  for (var i=0;i<vars.length;i++) {
			var pair = vars[i].split("=");
				// If first entry with this name
			if (typeof query_string[pair[0]] === "undefined") {
			  query_string[pair[0]] = decodeURIComponent(pair[1]);
				// If second entry with this name
			} else if (typeof query_string[pair[0]] === "string") {
			  var arr = [ query_string[pair[0]],decodeURIComponent(pair[1]) ];
			  query_string[pair[0]] = arr;
				// If third or later entry with this name
			} else {
			  query_string[pair[0]].push(decodeURIComponent(pair[1]));
			}
		  } 
			return query_string;
		}(),
		
		get_url_param: function(name, location) {// {{{
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
		},
		
		get_url_params: function(location) {// {{{
			var urlFragments;
			try {
				if (location) {
					if (location.indexOf("?") == -1) {
						location = "?" + location;
					}
					urlFragments = location.split("?")[1].split("&");
				} else {
					urlFragments = window.location.href.split("?")[1].split("&");
				}
			}
			catch (err) {
				return {};
			}

			var params = new Object();

			for (var i = 0; i < urlFragments.length; i++) {
				var param = urlFragments[i].split("=");
				params[unescape(param[0])] = param[1];
			}

			return params;

		},
		
		
	};
	
	return NVR;
});