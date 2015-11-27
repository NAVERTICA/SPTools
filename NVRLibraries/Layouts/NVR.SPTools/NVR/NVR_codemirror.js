define(['jquery', 'underscore', 'nvr'/*,...*/], function ($, _, NVR/*,...*/) {
	"use strict";
	//asi uz neni potreba
	NVR.codemirror = {
		codemirrorLanguageChanged: function(cmName, sel) {
			if (NVR._codemirror[cmName]) {
			    NVR._codemirror[cmName].setOption("mode", sel);
			    NVR._codemirror[cmName].focus();
			}
		},

		codemirrorAutoselectParser: function(content) {
			// hleda stredniky na koncich radku (i kdyz jsou za nimi komentare) nebo var na zacatku
			var jspattern = /[;](([\s]*)|([\s]*\/\/.*)|([\s]*\/\*.*))?[\r\n]+|^([\s]*)var/;
			// hleda dvojtecku na konci radku (i kdyz je za ni komentar), nebo def nebo # na zacatku radku
			var pypattern = /[:](([\s])|([\s]*#.*))[\r\n]+|^([\s]*)def|^([\s]*)#/;
			// hleda html/xml tagy
			var htmlpattern = /<[_:A-Za-z][-._:A-Za-z0-9]*(\s+[_:A-Za-z][-._:A-Za-z0-9]*\s*=\s*("[^"]*"|'[^']*'))*\s*>/;

			if (content.indexOf("ExecScript(") != -1 || content.toString().match(jspattern)) {
				return "javascript";
			} else if (content.match(pypattern)) {
				return "python";
			} else if (content.match(htmlpattern)) {
				return "htmlmixed";
				// json?
			} else if ((content.indexOf("{") != -1 && content.indexOf("}") != -1) || (content.indexOf("[") != -1 && content.indexOf("]") != -1)) {
				return "javascript";
			} else {
				return "";
			}
		},

		codemirrorGetSelectBox: function(cmName) {
			return "<select id='languageSelect' onchange='codemirrorLanguageChanged(\"" + cmName + "\", this.options[this.selectedIndex].value); return false;'>"
				+ "<option value=''>None</option>"
				+ "<option value='htmlmixed'>HTML</option>"
				+ "<option value='python'>Python</option>"
				+ "<option value='javascript'>JavaScript</option>"
				+ "<option value='css'>CSS</option>"
				//+"<option value='SqlParser'>SQL</option>"
				+ "<option value='clike'>C#</option>"
				+ "</select> ";
		}
	};
	
	return NVR;
});