
// <param name="tabTitle" type="String"></param>
// <param name="groupName" type="String"></param>
// <param name="toolTipDescription" type="String"></param>
// <param name="toolTipTitle" type="String"></param>
// <param name="labelText" type="String"></param>
// <param name="image32by32Url" type="String"></param>
// <param name="clickFunction" type="Function"></param>

//testovaci buttonObject
var buttonObject = 
[
{
	tabTitle: 'Library',
	groupName: 'Custom Buttons',
	button: 
	[
	{
		toolTipDescription: 'You are being watched.',
		toolTipTitle: 'Click Me!',
		labelText: 'Watch out!',
		image32by32Url: '_layouts/15/images/placeholder32x32.png',
		clickFunction: function(){alert('buch');},
	},
	{
		toolTipDescription: 'Hlavne pomalu.',
		toolTipTitle: 'Click Me instead!',
		labelText: 'Opatrne',
		image32by32Url: '_layouts/15/images/CRIT_32.GIF',
		clickFunction: function(){alert('pozor.');},
	}
	]
},
{
	tabTitle: 'Files',
	groupName: 'Trochu jine Buttons',
	button: 
	[
	{
		toolTipDescription: 'You are being watched.',
		toolTipTitle: 'Click Me!',
		labelText: 'Buch!',
		image32by32Url: '_layouts/15/images/placeholder32x32.png',
		clickFunction: function(){alert('buch');},
	},
	{
		toolTipDescription: 'Hlavne pomalu.',
		toolTipTitle: 'Click Me instead!',
		labelText: 'Smula',
		image32by32Url: '_layouts/15/images/CRIT_32.GIF',
		clickFunction: function(){alert('pozor.');},
	}
	]
},
{
	tabTitle: 'Library',
	groupName: 'Trochu jine Buttons',
	button: 
	[
	{
		toolTipDescription: 'You are being watched.',
		toolTipTitle: 'Click Me!',
		labelText: 'trisk!',
		image32by32Url: '_layouts/15/images/placeholder32x32.png',
		clickFunction: function(){alert('buch');},
	},
	{
		toolTipDescription: 'Hlavne pomalu.',
		toolTipTitle: 'Click Me instead!',
		labelText: 'prask',
		image32by32Url: '_layouts/15/images/CRIT_32.GIF',
		clickFunction: function(){alert('smula nedaval jsi pozor.');},
	}
	]
}
];

	
function createRibbonButtons(buttonObject){
for(var i = 0; i<buttonObject.length; i++){
	waitForElement(buttonObject[i]);
}
function waitForElement(buttonObject){
var ribbon = (SP.Ribbon.PageManager.get_instance()).get_ribbon();
if(ribbon != null){var title = ribbon.$1m_3.$1W_0;}
    if(ribbon != null && title == buttonObject.tabTitle){

		var tab = ribbon.getChildByTitle(buttonObject.tabTitle);
		var group = new CUI.Group(ribbon, 'Custom.Tab.Group', buttonObject.groupName, 'Group Description', 'Custom.Custom.Command', null);
		tab.addChild(group);
		var layout = new CUI.Layout(ribbon, "Custom.Layout", "The Layout");
		group.addChild(layout);
		var section = new CUI.Section(ribbon, 'Custom.Section', 2, 'Top'); 
		layout.addChild(section);
		for(var i =0; i<buttonObject.button.length;i++){
				var controlProperties = new CUI.ControlProperties();
				controlProperties.Command = 'Custom.Button.Command';
				controlProperties.Id = 'Custom.ControlProperties';
				controlProperties.TemplateAlias = 'o1';
				controlProperties.ToolTipDescription = buttonObject.button[i].toolTipDescription;
				controlProperties.Image32by32 = buttonObject.button[i].image32by32Url;
				controlProperties.ToolTipTitle = buttonObject.button[i].toolTipTitle;
				controlProperties.LabelText = buttonObject.button[i].labelText;

				var button = new CUI.Controls.Button(ribbon, 'Custom.Button', controlProperties);

				var controlComponent = button.createComponentForDisplayMode('Large');

				var row1 = section.getRow(1);

				row1.addChild(controlComponent);
				group.selectLayout('The Layout');
				ribbon.pollForStateAndUpdate();

				CUI.Utility.setEnabledOnElement(button.$E_1, true);
				button.$1K_0 = true;

				button.$$d_onClick = buttonObject.button[i].clickFunction;
				$addHandler(button.$E_1,'click',button.$$d_onClick);
		}
    }
    else{
        setTimeout(function(){
		//ribbon = (SP.Ribbon.PageManager.get_instance()).get_ribbon();
            waitForElement(buttonObject);
			//console.log('waiting...');
        },250);
    }
}
}
/***********************************************/