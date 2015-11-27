(function () {
var fieldCtx = {};
fieldCtx.Templates = {};

fieldCtx.Templates.Fields = 
	{
		"StartDate": {
			"EditForm": StartDateEditTemplate,
			"NewForm": StartDateNewTemplate
		},
		"NVR_EndDate": {
			"EditForm": EndDateEditTemplate,
			"NewForm": EndDateNewTemplate
		},
		"NVR_Days": {
			"EditForm": DaysEditTemplate,
			"NewForm": DaysNewTemplate
		}
	};
	
	SPClientTemplates.TemplateManager.RegisterTemplateOverrides(fieldCtx);
})();
function EndDateNewTemplate(ctx) {
    var formCtx = SPClientTemplates.Utility.GetFormContextForCurrentField(ctx);
	
	formCtx.registerGetValueCallback(formCtx.fieldName, function () {
		var tempo = document.getElementById("endDate").value.split('/');
		tempo[0] = tempo.splice(1,1,tempo[0])[0];
		var result = tempo.join('. ')+' '+document.getElementById("timepickerTo").value;
        return result;
    });
	
	var endDiv = "<input id='endDate' style='display: none;'>";
	var timepicker = "<br/><span class='ms-metadata'>End:</span><input id='timepickerTo'>";

	return endDiv + timepicker;
}
function StartDateNewTemplate(ctx) {
    var formCtx = SPClientTemplates.Utility.GetFormContextForCurrentField(ctx);
	formCtx.registerGetValueCallback(formCtx.fieldName, function () {
		var tempo = document.getElementById("startDate").value.split('/');
		tempo[0] = tempo.splice(1,1,tempo[0])[0];
		var result = tempo.join('. ')+' '+document.getElementById("timepickerFrom").value;
        return result;
    });
	
	var calendarDiv = "<div id='calendar'></div>"
	var startDiv = "<input id='startDate' style='display: none;'>";
	var timepicker = "<br/><span class='ms-metadata'>Start:</span><input id='timepickerFrom'>";

	return calendarDiv + startDiv + timepicker ;
}
function DaysNewTemplate(ctx){
	var formCtx = SPClientTemplates.Utility.GetFormContextForCurrentField(ctx);
	formCtx.registerGetValueCallback(formCtx.fieldName, function () {
        return document.getElementById("duration").value;
    });
	var durationDiv = "<input id='duration'>";
	return durationDiv;
}

function EndDateEditTemplate(ctx) {
    var formCtx = SPClientTemplates.Utility.GetFormContextForCurrentField(ctx);
	//upraveni datumu
	var temp = formCtx.fieldValue.replace(/ /g,'').split('.')
	temp[2] = temp[2].substring(0,4);
	temp[0] = temp.splice(1,1,temp[0])[0];
	var dateValue = temp.join('/');
	var time = formCtx.fieldValue.split(' ')[3];
	if(time.split(':')[0].length == 1){time='0'+time;}
	if(time.split(':')[1].length == 1){time=time+'0';}
	//
	
	formCtx.registerGetValueCallback(formCtx.fieldName, function () {
		var tempo = document.getElementById("endDate").value.split('/');
		tempo[0] = tempo.splice(1,1,tempo[0])[0];
		var result = tempo.join('. ')+' '+document.getElementById("timepickerTo").value;
        return result;
    });
	
	var endDiv = "<input id='endDate' value='"+dateValue+"' style='display: none;'>";
	var timepicker = "<br/><span class='ms-metadata'>End:</span><input id='timepickerTo' value='"+time+"'>";

	return endDiv + timepicker;
}
function StartDateEditTemplate(ctx) {
    var formCtx = SPClientTemplates.Utility.GetFormContextForCurrentField(ctx);
	//upraveni datumu
	var temp = formCtx.fieldValue.replace(/ /g,'').split('.')
	temp[2] = temp[2].substring(0,4);
	temp[0] = temp.splice(1,1,temp[0])[0];
	var dateValue = temp.join('/');
	var time = formCtx.fieldValue.split(' ')[3];
	if(time.split(':')[0].length == 1){time='0'+time;}
	if(time.split(':')[1].length == 1){time=time+'0';}
	//
	
	formCtx.registerGetValueCallback(formCtx.fieldName, function () {
		var tempo = document.getElementById("startDate").value.split('/');
		tempo[0] = tempo.splice(1,1,tempo[0])[0];
		var result = tempo.join('. ')+' '+document.getElementById("timepickerFrom").value;
        return result;
    });
	
	var calendarDiv = "<div id='calendar'></div>"
	var startDiv = "<input id='startDate' value='"+dateValue+"' style='display: none;'>";
	var timepicker = "<br/><span class='ms-metadata'>Start:</span><input id='timepickerFrom' value='"+time+"'>";

	return calendarDiv + startDiv + timepicker ;
}
function DaysEditTemplate(ctx){
	var formCtx = SPClientTemplates.Utility.GetFormContextForCurrentField(ctx);
	formCtx.registerGetValueCallback(formCtx.fieldName, function () {
        return document.getElementById("duration").value;
    });
	var durationDiv = "<input id='duration' value='"+formCtx.fieldValue+"'>";
	return durationDiv;
}

function createCalendar(){
	var formCtx = SPClientTemplates.Utility.GetFormContextForCurrentField(ctx);
	
	$("#calendar").continuousCalendar({
		startField: $("#startDate"), 
		endField: $("#endDate"),
		isRange: true
		}).on('calendarChange', function() {
		  var container = $(this);
		  var range = container.calendarRange();
		  var dur = Math.round(( range.end.date - range.start.date ) / 86400000);
		  $('#duration')[0].value=dur;
    });
	setTimeout(function(){$('.label')[1].style.display='none';},100);

}
function createTimepicker(){
	$("#timepickerFrom").timepicker({ 'timeFormat': 'H:i','step': 15,'forceRoundTime': true });
	$("#timepickerTo").timepicker({ 'timeFormat': 'H:i','step': 15,'forceRoundTime': true });
}
function loadScript(url, callback) {
    var script = document.createElement("script")
    script.type = "text/javascript";
    if (script.readyState) { //IE
        script.onreadystatechange = function () {
            if (script.readyState == "loaded" || script.readyState == "complete") {
                script.onreadystatechange = null;
                callback();
            }
        };
    } else { //Others
        script.onload = function () {
            callback();
        };
    }
    script.src = url;
    document.getElementsByTagName("head")[0].appendChild(script);
}
function loadCss(url) {
    var css = document.createElement('link');
    css.rel = 'stylesheet';
    css.href = url;
    document.getElementsByTagName("head")[0].appendChild(css);
}
loadCss('/_layouts/15/NVR.SPTools/CSRAssets/VacationsCalendarResources/jquery.continuousCalendar-latest-min.css');
loadCss('/_layouts/15/NVR.SPTools/CSRAssets/VacationsCalendarResources/jquery.timepicker.css');
loadScript('/_layouts/15/NVR.SPTools/CSRAssets/VacationsCalendarResources/jquery.debug.js', function(){
	$("nobr:contains('Start Date')").text("Date Range");
	$("nobr:contains('End date')").text("Time");
	$(".ms-metadata:contains('Vacation end date')")[0].style.display = 'none';
	loadScript('/_layouts/15/NVR.SPTools/CSRAssets/VacationsCalendarResources/jquery.timepicker.min.js', function(){
		createTimepicker();
	});
	loadScript('/_layouts/15/NVR.SPTools/CSRAssets/VacationsCalendarResources/jquery.continuousCalendar-latest-min.js', function(){
		createCalendar();
	});
});
    