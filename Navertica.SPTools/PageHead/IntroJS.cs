/*  Copyright (C) 2015 NAVERTICA a.s. http://www.navertica.com 

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.  */
	
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI;
using Microsoft.SharePoint;
using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.RepoService;

namespace Navertica.SharePoint.PageHead
{
	public class IntroConfig : ConfigBranch
	{
		public class IntroStep
		{
			public string Element;
			public List<LanguageTitle> Intro;
			public string Position;
		}

		public string Theme;
		public List<LanguageTitle> Title;
		public List<IntroStep> Steps;
	}

	public class IntroJS : UserControl, IPageHead
	{
		public string Category()
		{
			return "IntroJS";
		}

		public object LoadContents(ConfigServiceClient cfg, RepoServiceClient repo)
		{
			StringBuilder sb = new StringBuilder();
			var cfgScripts = GetConfigGuidList(cfg);

			if (cfgScripts.Count == 0) return null;

			sb.AppendLine("<script type='text/javascript' src='/_layouts/15/NVR.SPTools/libs/Intro/intro.js'></script>").AppendLine();
			sb.AppendLine("<link type=\"text/css\" rel=\"stylesheet\" href=\"/_layouts/15/NVR.SPTools/libs/Intro/introjs.css\" />");

			sb.AppendLine("<script type='text/javascript'>");
			sb.AppendLine(@"
			function ReplaceHelp(){
				window.helpDiv = document.createElement('div');
				helpDiv.className = 'ms-core-menu-box ms-core-defaultFont';
				window.helpUl = document.createElement('ul');
				helpUl.className = 'ms-core-menu-list';
				helpDiv.appendChild(helpUl);
				window.createHelpLi= function(fn, title){
					var li = document.createElement('li');
					li.className='ms-core-menu-item';
					var a = document.createElement('a');
					a.className = 'ms-core-menu-link';
					a.href = 'javascript:;';
					a.title = title;
					a.innerText = title;
					a.onclick = function(){window.helpDialog.close(); fn(); return false;}
					li.appendChild(a);
			
					window.helpUl.appendChild(li);
				}
				var help = $('span#ms-help a');
				createHelpLi(function(){TopHelpButtonClick('HelpHome','');return false;}, 'Default Help');
				help[0].onclick = function(){
					window.helpDialog = SP.UI.ModalDialog.showModalDialog({
						title:'Help Menu',
						x:window.innerWidth-350,
						y:0,
						html: helpDiv,
						autoSize: true,
						showClose: true,
					});
					return false;
				};
				SP.SOD.notifyScriptLoadedAndExecuteWaitingJobs('helpReplace');
			}
			SP.SOD.executeFunc('sp.ui.dialog.js', 'SP.UI.ModalDialog', function(){_spBodyOnLoadFunctionNames.push('ReplaceHelp');});
			");
			sb.AppendLine("</script>");

			foreach (Guid cfgId in cfgScripts)
			{
				ConfigBranch c = cfg.GetBranch<ConfigBranch>(cfgId, this.Category());

				IntroConfig introConfig = ConfigBranch.GetInstance<IntroConfig>(c);
				string theme = !string.IsNullOrEmpty(introConfig.Theme) ? introConfig.Theme : "introjs-nassim";
				sb.AppendLine("<link type=\"text/css\" rel=\"stylesheet\" href=\"/_layouts/15/NVR.SPTools/libs/Intro/themes/" + theme + ".css\" />");

				string functionName = "startIntro" + cfgId.ToString("N");
				sb.AppendLine("<script type='text/javascript'>");
				sb.AppendLine("function " + functionName + "(){");
				sb.AppendLine("var intro = introJs();");
				sb.AppendLine("intro.setOptions({ steps: [");

				string cultureName = CultureInfo.CurrentUICulture.Name;
				for (int i = 0; i < introConfig.Steps.Count; i++)
				{
					IntroConfig.IntroStep introStep = introConfig.Steps[i];
					string intro = "Intro " + cultureName;

					sb.AppendLine("{");

					if (introStep.Intro != null)
					{
						var introTitle = introStep.Intro.SingleOrDefault(l => l.Language == cultureName);
						if (introTitle != null)
						{
							intro = introTitle.Title;
						}
					}

					if (!string.IsNullOrEmpty(introStep.Element))
					{
						sb.AppendLine("element: " + introStep.Element + ",");
					}

					if (!string.IsNullOrEmpty(introStep.Position))
					{
						sb.AppendLine("position: \"" + introStep.Position + "\",");
					}

					sb.AppendLine("intro: \"" + intro + "\"");

					sb.AppendLine("}");

					if (i < introConfig.Steps.Count - 1)
					{
						sb.AppendLine(",");
					}
				}

				sb.AppendLine("]});");
				sb.AppendLine("intro.start();");
				sb.AppendLine("}");

				string title = "help";
				if (introConfig.Title != null)
				{
					var helpTitle = introConfig.Title.SingleOrDefault(l => l.Language == cultureName);
					if (helpTitle != null)
					{
						title = helpTitle.Title;
					}
				}
				//function(){}
				sb.AppendLine("SP.SOD.executeOrDelayUntilScriptLoaded( function(){createHelpLi(" + functionName + ",'" + title + "')},'helpReplace');");
				//sb.AppendLine("SP.SOD.executeFunc('sp.ui.dialog.js', 'SP.UI.ModalDialog', function(){_spBodyOnLoadFunctionNames.push(\"createHelpLi(" + functionName + ",'" + title + "')\");});");
				sb.AppendLine("</script>");
			}
			return sb.ToString();
		}

		public ConfigGuidList GetConfigGuidList(ConfigServiceClient cfg)
		{
			var filters = new Dictionary<ConfigFilterType, string>
			{
				{ ConfigFilterType.App, Category() },
				{ ConfigFilterType.Url, this.Context.Request.RawUrl }
			};

			if (SPContext.Current.List != null)
			{
				filters.Add(ConfigFilterType.ListType, SPContext.Current.List.BaseType.ToString());
			}

			if (SPContext.Current.ListItem != null)
			{
				filters.Add(ConfigFilterType.ContentType, SPContext.Current.ListItem.ContentTypeId.ToString());
			}

			return cfg.Filter(SPContext.Current.Web, filters);
		}
	}
}

/*
 {
  "IntroJS": {
	"Theme": "introjs-dark",
	"Title": [
	  {
		"Language": "cs-CZ",
		"Title": "Ahoj světe"
	  },
	  {
		"Language": "en-US",
		"Title": "Hello World"
	  },
	  {
		"Language": "ru-RU",
		"Title": "RUSSIANS WORLD"
	  },
	  {
		"Language": "sk-SK",
		"Title": "Ahoj světík po slovensky"
	  }
	],
	"Steps": [
	  {
		"Intro": [
		  {
			"Language": "cs-CZ",
			"Title": "INTRO START"
		  },
		  {
			"Language": "en-US",
			"Title": "INTRO ZACATEK"
		  }
		],
		"Position": "top",
		"Element": ""
	  },
	  {
		"Intro": [
		  {
			"Language": "cs-CZ",
			"Title": "VIEW CZ"
		  },
		  {
			"Language": "en-US",
			"Title": "VIEW EN"
		  }
		],
		"Position": "bottom-right-aligned",
		"Element": "#DeltaPlaceHolderMain"
	  }
	]
  }
}
 */