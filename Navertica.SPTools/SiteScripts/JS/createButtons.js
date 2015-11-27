function createButtons(controlProperties) {
    // Button Element https://msdn.microsoft.com/en-us/library/office/ff458366.aspx 
    window.ribbonInitDef = new $.Deferred();
    //check inited ribbon handler
    SP.Ribbon.PageManager.get_instance().add_ribbonInited(function () {
        if (typeof (ribbonInitDef) == 'object') {
            if (ribbonInitDef.state() == 'pending') {
                ribbonInitDef.resolve();
            }
        }
    });
    //add Tab init load check
    var b = CUI.Ribbon.prototype.$1q_0.toString();
    b = b.substring(0, b.length - 1);
    b = b.replace("f", "(function(){return f");
    b += "if ($v_0 != null) {"
        + "if (typeof (ribbonInitDef[$v_0.$4_0]) == 'object') {"
        + "if (ribbonInitDef[$v_0.$4_0].state() == 'pending') {"
        + "console.log('resolving... ' + $v_0.$4_0);"
        + "ribbonInitDef[$v_0.$4_0].resolve();"
        + "}"
        + "}"
        + "}"
        + "};})();";

    CUI.Ribbon.prototype.$1q_0 = eval(b);
    
    $.each(controlProperties, function (i, el) {
        //register commands
        var customcommand = {
            name: "CommandUIHandler",
            attrs: {
                Command: controlProperties[i].commandID,
                CommandAction: "javascript: " + controlProperties[i].actionScript,
                EnabledScript: "javascript: " + controlProperties[i].enabledScript,
                WebPartId: "WebPart" + ctx.wpq
            }
        };
        window.g_commandUIHandlers.children.push(customcommand);
    });
    /***NEW***/

    //Create CUI Objects
    _ribbonStartInit('Ribbon.Read', true, null);


    $.when(ribbonInitDef).then(function () {
        console.log('init Ribbon Loaded');
        var ribbon = (SP.Ribbon.PageManager.get_instance()).get_ribbon();
        $.each(controlProperties, function (i, el) {
            if (typeOf(controlProperties[i].tab) == 'string') {
                var tabObj = ribbon.getChildByTitle(controlProperties[i].tab);

                ribbonInitDef[tabObj.$4_0] = new $.Deferred();

                if (tabObj.get_needsDelayIniting()) {
                    tabObj.doDelayedInit();
                } else {
                    ribbonInitDef[tabObj.$4_0].resolve();
                }
            } else {
                /*NEW TAB*/
            }

            $.when(ribbonInitDef[tabObj.$4_0]).then(function () {
                console.log(tabObj.$4_0 + " init loaded");
                if (typeOf(controlProperties[i].group) == 'string') { var groupObj = tabObj.getChildByTitle(controlProperties[i].group); }
                else {
                    /*NEW GROUP*/
                }
                /**/
                var controlPropertiesObj = new CUI.ControlProperties();
                controlPropertiesObj.Command = controlProperties[i].commandID;
                controlPropertiesObj.Id = controlProperties[i].controlPropertiesID;
                controlPropertiesObj.TemplateAlias = 'o1';
                controlPropertiesObj.ToolTipDescription = controlProperties[i].toolTipDescription;
                controlPropertiesObj.Image32by32 = controlProperties[i].image32;
                controlPropertiesObj.Image16by16 = controlProperties[i].image16;
                controlPropertiesObj.ToolTipTitle = controlProperties[i].toolTipTitle;
                controlPropertiesObj.ToolTipHelpKeyWord = controlProperties[i].toolTipHelpKeyWord;
                controlPropertiesObj.ToolTipShortcutKey = controlProperties[i].toolTipShortcutKey;
                controlPropertiesObj.LabelText = controlProperties[i].labelText;
                /**/
                /** insert section in all layouts */
                var a = groupObj.$6_0.getEnumerator();
                var counter = 0;
                var sectionArr = [];
                while (a.moveNext()) {
                    var b = a.get_current();
                    var layout = b;
                    //console.log(b);
                    if (layout.$1W_0 != 'GroupPopupLayout') {
                        var sectionObj = (new CUI.Section(ribbon, (controlProperties[i].sectionId + counter), (parseInt(controlProperties[i].sectionRows)), 1));
                        sectionArr.push(sectionObj);
                        layout.addChild(sectionObj);
                        counter++;
                    }
                }
                /**/
                $.each(sectionArr, function (f, el) {
                    if (this.get_parent().$1W_0 == 'LargeLarge' || this.get_parent().$1W_0 == 'MediumLarge' || this.get_parent().$1W_0 == 'SmallLarge') {
                        var button = new CUI.Controls.Button(ribbon, (controlProperties[i].buttonID + f), controlPropertiesObj);
                        var controlComponent = button.createComponentForDisplayMode('Large');
                        var rowObj = this.getRow(parseInt(controlProperties[i].rowNumber));
                        rowObj.addChild(controlComponent);
                    }
                    if (this.get_parent().$1W_0 == 'LargeMedium' || this.get_parent().$1W_0 == 'MediumMedium' || this.get_parent().$1W_0 == 'SmallMedium') {
                        var button = new CUI.Controls.Button(ribbon, (controlProperties[i].buttonID + f), controlPropertiesObj);
                        var controlComponent = button.createComponentForDisplayMode('Medium');
                        var rowObj = this.getRow(parseInt(controlProperties[i].rowNumber));
                        rowObj.addChild(controlComponent);
                    }
                    if (this.get_parent().$1W_0 == 'LargeSmall' || this.get_parent().$1W_0 == 'MediumSmall' || this.get_parent().$1W_0 == 'SmallSmall') {
                        var button = new CUI.Controls.Button(ribbon, (controlProperties[i].buttonID + f), controlPropertiesObj);
                        var controlComponent = button.createComponentForDisplayMode('Small');
                        var rowObj = this.getRow(parseInt(controlProperties[i].rowNumber));
                        rowObj.addChild(controlComponent);
                    }
                });
                ribbon.selectTabById('Ribbon.Read');
                ribbon.pollForStateAndUpdate();
            });
        });
    });
}