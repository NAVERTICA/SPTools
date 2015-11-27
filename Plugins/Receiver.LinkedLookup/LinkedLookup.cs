using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.SharePoint;
using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.Extensions;
using Navertica.SharePoint.Interfaces;
using Navertica.SharePoint.Logging;

namespace Navertica.SharePoint.Plugins
{
	public class LinkedLookup : IPlugin 
	{
		private ILogging log = NaverticaLog.GetInstance();

		#region Plugin Settings

		public List<PluginHost> PluginScope()
		{
			return new List<PluginHost> { new ItemReceiverPluginHost() };
		}

		public string Name()
		{
			return this.GetType().Name;
		}

		public string Description()
		{
			return "LinkedLookup receiver";
		}

		public string Version()
		{
			return "1.0";
		}

		public IPlugin GetInstance()
		{
			return new LinkedLookup();
		}

		#endregion

		public string Install(IEnumerable<string> urls, IDictionary<string, object> context)
		{
			//TODO
			return null;
			/*string res = "";
			List<SPEventReceiverType> events = new List<SPEventReceiverType>() { SPEventReceiverType.ItemAdded, SPEventReceiverType.ItemUpdating, SPEventReceiverType.ItemDeleting };

			foreach (var url in urls)
			{
				try
				{
					using (SPWeb web = ( context["site"] as SPSite ).OpenW(url, true))
					{
						SPList list = web.OpenList(url, true);
						foreach (var eve in events)
						{
							if (list.AttachEventReceiver(Const.NVR_DLL, "Navertica.SharePoint.Receivers.ItemReceiver", eve))
							{
								res += "Adedd " + eve + " at " + list.AbsoluteUrl() + "\n";
							}
						}
					}
				}
				catch (Exception exc)
				{
					log.LogException(exc, "Failed to install plugin " + this.Name());
				}
			}

			if (res != "")
			{
				log.LogInfo(res);
			}

			return res;*/
		}

		public IList<KeyValuePair<bool, string>> Test(ConfigBranch config, object context)
		{
			throw new NotImplementedException();
		}

		public class LinkedLookupConfig : ConfigBranch
		{
			private List<LinkedLookupSettings> _linkedLookup;

			public List<LinkedLookupSettings> LinkedLookup
			{
				get { return _linkedLookup; }
				set { _linkedLookup = value; }
			}
		}

		public class LinkedLookupSettings
		{
			private bool _regularUpdate;

			public bool RegularUpdate
			{
				get { return _regularUpdate; }
				set { _regularUpdate = value; }
			}

			private bool _force;

			public bool Force
			{
				get { return _force; }
				set { _force = value; }
			}

			private bool _updateVersion;

			public bool UpdateVersion
			{
				get { return _updateVersion; }
				set { _updateVersion = value; }
			}

			private string _localWeb;

			public string LocalWeb
			{
				get { return _localWeb; }
				set { _localWeb = value; }
			}

			private string _localList;

			public string LocalList
			{
				get { return _localList; }
				set { _localList = value; }
			}

			private string _remoteWeb;

			public string RemoteWeb
			{
				get { return _remoteWeb; }
				set { _remoteWeb = value; }
			}

			private string _remoteList;

			public string RemoteList
			{
				get { return _remoteList; }
				set { _remoteList = value; }
			}

			private string _localField;

			public string LocalField
			{
				get { return _localField; }
				set { _localField = value; }
			}

			private string _remoteField;

			public string RemoteField
			{
				get { return _remoteField; }
				set { _remoteField = value; }
			}
		}

		public object Execute(ConfigBranch branch, object context)
		{
			if (context == null) throw new ArgumentNullException("context");

			var config = ConfigBranch.GetInstance<LinkedLookupConfig>(branch);

			SPItemEventProperties props = null;
			SPEventReceiverType eventType;
			SPSite site;
			SPWeb web;
			SPList list;
			SPListItem item;

			#region Load properties

			if (context is SPItemEventProperties)
			{
				props = context as SPItemEventProperties;
				eventType = props.EventType;
				site = props.Site;
				web = props.Web;
				list = props.List;
				item = props.ListItem;
			}
			else if (context is SPListItem) // pro castecne ucely debugu
			{
				item = context as SPListItem;
				eventType = SPEventReceiverType.ItemDeleting;
				site = item.Web.Site;
				web = item.Web;
				list = item.ParentList;
			}
			else
			{
				log.LogError("Incorrect context");
				return null;
			}

			#endregion

			for (int i = 0; i < config.LinkedLookup.Count; i++)
			{
				LinkedLookupSettings settings = config.LinkedLookup[i];

				try
				{
					using (SPWeb localWeb = site.OpenW(settings.LocalWeb, true))
					{
						if (localWeb.ID == web.ID)
						{
							SPList localList = localWeb.OpenList(settings.LocalList, true);

							if (localList.ID == list.ID) // mame konfiguraci na zpracovavanou polozku a jdem dal
							{
								log.LogInfo("RUNNING LinkedLookup at " + localList.DefaultViewUrlNoAspx() + " on item " + item.ID);

								SPField localField = localList.OpenField(settings.LocalField, true);
								Guid lookupWebId = ( (SPFieldLookup) localField ).LookupWebId;
								Guid lookupListId = new Guid(( (SPFieldLookup) localField ).LookupList);

								item.Web.Site.RunElevated(delegate(SPSite elevatedSite)
								{
									using (SPWeb remoteWeb = elevatedSite.OpenW(lookupWebId, true))
									{
										SPList remoteList = remoteWeb.OpenList(settings.RemoteList, true);
										SPField remoteField = remoteList.OpenField(settings.RemoteField, true);

										string localLookupValue = "";
										string oldLookup = "";
										List<int> oldItems = new List<int>(); //ID polozek, ktere v remote lookupu byly driv
										List<int> newItems = new List<int>(); // ID polozek, ktere v remote lookupu maji byt po zmene

										try
										{
											if (eventType == SPEventReceiverType.ItemDeleting)
											{
												oldLookup = ( item[localField.InternalName] ?? "" ).ToString().Trim();
												localLookupValue = oldLookup;
											}
											else if (( eventType == SPEventReceiverType.ItemAdded || eventType == SPEventReceiverType.ItemUpdating ))
											{
												oldLookup = ( item[localField.InternalName] ?? "" ).ToString().Trim();
												localLookupValue = (props.AfterProperties[localField.InternalName] ?? "").ToString().Trim();
											}

											if (settings.Force)
											{
												localLookupValue = oldLookup;
												oldLookup = "";
											}

											// staré vazby v lokálním lookupu, které budeme chtít prověřit a případně smazat
											if (eventType == SPEventReceiverType.ItemDeleting || eventType == SPEventReceiverType.ItemUpdating)
											{
												if (oldLookup != "")
												{
													oldItems = oldLookup.GetLookupIndexes().ToList();
												}
											}

											// nové vazby v lokálním lookupu, plus zajištění, že nesmažeme něco, co má zůstat navázané
											// !!! POZOR, pokud je lookup schovany z editFormu pomoci Advanced Field Modifications, bude v AfterProperties prazdny - max. jde pouzit skryti radku ve formu javascriptem
											if (( eventType == SPEventReceiverType.ItemAdded || eventType == SPEventReceiverType.ItemUpdating ))
											{
												newItems = localLookupValue.GetLookupIndexes().ToList();
											}

											List<int> addItems = newItems.Where(id => !oldItems.Contains(id)).ToList();
											List<int> removeItems = oldItems.Where(id => !newItems.Contains(id)).ToList();

											if (removeItems.Count > 0)
											{
												//if (config.DebugOn()) Tools.Log(props, config, "Clearing old - items " + removeItems.JoinStrings(", "));

												ClearOld(settings, item.ID, removeItems, remoteField, remoteList);
											}
											if (addItems.Count > 0)
											{
												//if (config.DebugOn()) Tools.Log(props, config, "Adding new - items " + addItems.JoinStrings(", "));

												AddNew(settings, props, addItems, remoteField, remoteList);
											}

											// if (removeItems.Count == 0 && addItems.Count == 0 && config.DebugOn()) Tools.Log(props, config, "LinkedLookupReceiver - no changes written");
										}
										catch (Exception exc)
										{
											log.LogException(exc);

										}

									}

									return null;

								});
							}
						}
					}
				}
				catch (Exception exc)
				{
					log.LogException(exc);
				}
			}

			return "OK";
		}

		private static void ClearOld(LinkedLookupSettings config, int listItemId, List<int> removeItems, SPField remoteLookup, SPList remoteList)
		{
			foreach (int id in removeItems)
			{
				SPListItem remoteItem;
				try
				{
					remoteItem = remoteList.GetItemById(id);
				}
				catch
				{
					continue;
				}

				// pokusit se z lookupu v remoteItemu smazat polozku, na ktere aktualne bezi receiver
				// pokud je to multilookup, chceme si zachovat zbytek hodnot
				if (( (SPFieldLookup) remoteLookup ).AllowMultipleValues && ( remoteItem[remoteLookup.InternalName] ?? "" ).ToString().Trim() != "")
				{
					// stavajici obsah lookupu
					SPFieldLookupValueCollection existingCollection = new SPFieldLookupValueCollection(( remoteItem[remoteLookup.InternalName] ?? "" ).ToString());
					SPFieldLookupValue toRemove = existingCollection.FirstOrDefault(lookup => lookup.LookupId == listItemId);

					// ze stavajiciho obsahu odstranime polozku, na ktere bezi receiver
					if (toRemove != null) existingCollection.Remove(toRemove);

					// upraveny obsah vratime zpatky
					remoteItem[remoteLookup.InternalName] = existingCollection;
				}
					// pokud je to jednoduchy lookup, a neni prazdny, proste tam nastavime aktualni polozku
				else if (( remoteItem[remoteLookup.InternalName] ?? "" ).ToString().Trim() != "")
				{
					SPFieldLookupValue existingVal = new SPFieldLookupValue(( remoteItem[remoteLookup.InternalName] ?? "" ).ToString());
					if (existingVal.LookupId == listItemId)
					{
						remoteItem[remoteLookup.InternalName] = null;
					}
				}

				if (config.RegularUpdate)
				{
					remoteItem.Update(false);
				}
				else
				{
					// pokud jsou receivery na obou stranach, nechceme se dostat do smycky
					remoteItem.SystemUpdate(config.UpdateVersion, false);
				}
			}
		}

		private static void AddNew(LinkedLookupSettings config, SPItemEventProperties props, List<int> addItems, SPField remoteLookup, SPList remoteList)
		{
			foreach (int id in addItems)
			{
				// najit odpovidajici remote listitem
				SPListItem remoteItem;
				try
				{
					remoteItem = remoteList.GetItemById(id);
				}
				catch
				{
					continue;
				}

				// do kazde z nich pridal lookup na nasi polozku
				// pokud je remoteLookup multilookup, overime, ze tam jeste neni, a pridame ke stavajicim
				if (( (SPFieldLookup) remoteLookup ).AllowMultipleValues)
				{
					SPFieldLookupValueCollection existingCollection = ( remoteItem[remoteLookup.InternalName] ?? "" ).ToString().Trim() != "" ?
						new SPFieldLookupValueCollection(remoteItem[remoteLookup.InternalName].ToString()) :
						new SPFieldLookupValueCollection();

					// zkontrolovat, jestli už tam dané ID není, ale ID, ktere menime!!!!
					bool exists = existingCollection.Any(lookupVal => lookupVal.LookupId == props.ListItemId);

					if (!exists)
					{
						existingCollection.Add(new SPFieldLookupValue(props.ListItemId,
							( props.AfterProperties[( (SPFieldLookup) remoteLookup ).LookupField] ??
							  props.ListItem[( (SPFieldLookup) remoteLookup ).LookupField] ).ToString()));

						remoteItem[remoteLookup.InternalName] = existingCollection;
					}
				}
					// pokud je to single lookup, proste to tam pridame
				else
				{
					if (props.AfterProperties[( (SPFieldLookup) remoteLookup ).LookupField] != null)
					{
						SPFieldLookupValue newVal = new SPFieldLookupValue(props.ListItemId,
							props.AfterProperties[( (SPFieldLookup) remoteLookup ).LookupField].ToString());
						remoteItem[remoteLookup.InternalName] = newVal;
					}
					else
					{
						SPFieldLookupValue newVal = new SPFieldLookupValue(props.ListItemId,
							props.ListItem[( (SPFieldLookup) remoteLookup ).LookupField].ToString());
						remoteItem[remoteLookup.InternalName] = newVal;
					}
				}

				if (config.RegularUpdate)
				{
					remoteItem.Update(false);
				}
				else
				{
					// pokud jsou receivery na obou stranach, nechceme se dostat do smycky
					remoteItem.SystemUpdate(config.UpdateVersion, false);
				}
			}
		}
	}
}

/*
 {
	"LinkedLookup--I": {
		"LinkedLookup": [
			{
				"RegularUpdate": true,
				"Force": true,
				"UpdateVersion": true,
				"LocalWeb": "/cfp",
				"LocalList": "Tasks",
				"LocalField": "NVR_ParentTopic",
				"RemoteWeb": "/cfp",
				"RemoteList": "Topics",
				"RemoteField": "NVR_TopicTasks"
			},
			{
				"RegularUpdate": true,
				"Force": true,
				"UpdateVersion": true,
				"LocalWeb": "/cfp",
				"LocalList": "Topics",
				"LocalField": "NVR_TopicTasks",
				"RemoteWeb": "/cfp",
				"RemoteList": "Tasks",
				"RemoteField": "NVR_ParentTopic"
			}
		]
	},
	"ItemAdded": {
		"LinkedLookup--I": "LinkedLookup"
	},
	"ItemUpdating": {
		"LinkedLookup--I": "LinkedLookup"
	},
	"ItemDeleting": {
		"LinkedLookup--I": "LinkedLookup"
	}
}
 */