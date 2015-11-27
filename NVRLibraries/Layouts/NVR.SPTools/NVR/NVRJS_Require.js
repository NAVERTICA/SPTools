(function (root, factory) {
	"use strict";
	if(typeof define === 'function' && define.amd) {
		//do not create global:
		//define(['jquery'/*,...*/], factory);
		/***/
		// Also create a global in case some scripts
		// that are loaded still are looking for
		// a global even when an AMD loader is in use.
		/***/
	    define(["jquery", "underscore"], function ($) {
			return (root.NVR = factory($,_/*,...*/));
		});
	} else {
		root.NVR = factory(root.jQuery, root._)/*,...*/);
	}
}(this, function ($,_/*,...*/) {
	"use strict";
	
	var NVR/*,...*/;
	
	NVR = {
		/**demo**/
		displayError: function(msg){
			alert(msg);
		},/*,...*/
		/**globals**/
		globalContextMenuItems: [],// doplneni vlastnich polozek do kontextovych menu (namisto CustomActions)
		originalBackup: "",
		backup_data: {},
		__formCells: null,
		navert_notif: {},// uklada notifikace, ktere uz na teto strance byly otevreny
		backup_problem_growl: false,
		fbOldData: "",
		prefillDict: {},
		lookupValueAddingField: "",
		statusCloseCallbacks: {},// uklada SID jednotlivych statusu a dovoluje k jejich zavreni priradit funkci
		_codemirror: {},// uklada instance codemirroru
		dictFields: {},// uklada data pro DictionaryFieldy
		runOnEachPostback: [],// uklada funkce, ktere se maji poustet po kazdem postbacku (i u UpdatePanelu apod.)
		aggregatorLoaded: function () {},// prazdne, pro prepsani v inlinescriptu na strance s agregatorem
		runOnLoaded: function () {},  // prazdne, prddo prepsani ve strance
	}
	
	return NVR;
}));