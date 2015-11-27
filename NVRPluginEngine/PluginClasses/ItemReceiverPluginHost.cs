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
using Microsoft.SharePoint;

namespace Navertica.SharePoint
{
    public class ItemReceiverPluginHost : PluginHost
    {
        //we want to have the plugin in both the collection of ItemReceiverPluginHost, as well as in global PluginHost.Collection, so everything is overriden

        #region overrides

        internal new static Dictionary<string, List<IPlugin>> Collection = new Dictionary<string, List<IPlugin>>();

        public new static void Add(SPSite site, IPlugin plugin, bool force = false)
        {
            if (site == null) throw new ArgumentNullException("site");
            if (plugin == null) throw new ArgumentNullException("plugin");
            if (( plugin.Name().Length > 0 ) && !( plugin.Name().Equals("All") || plugin.Name().Equals("None") ))
            {
                if (List(site).Contains(plugin.Name()))
                {
                    if (force) Remove(site, plugin.Name());
                    else return;
                }
                if (!Collection.ContainsKey(site.Url))
                {
                    Collection.Add(site.Url, new List<IPlugin> { plugin });
                }
                else
                {
                    Collection[site.Url].Add(plugin);
                }
                PluginHost.Add(site, plugin, force);
            }
        }

        public new static List<string> List(SPSite site)
        {
            List<string> names = new List<string>();

            if (!Collection.ContainsKey(site.Url)) return names;

            foreach (IPlugin plugin in Collection[site.Url])
            {
                names.Add(plugin.Name());
            }
            return names;
        }

        public new static IPlugin Get(SPSite site, string name)
        {
            if (!Collection.ContainsKey(site.Url)) return null;
            foreach (IPlugin plugin in Collection[site.Url])
            {
                if (plugin.Name().Equals(name))
                {
                    IPlugin newInstance = plugin.GetInstance();
                    return newInstance;
                }
            }
            return null;
        }

        internal new static void Remove(SPSite site, string name)
        {
            int pluginIndex = 0;
            for (int i = 0; i < Collection[site.Url].Count; i++)
            {
                if (Collection[site.Url][i].Name().Equals(name))
                {
                    pluginIndex = i;
                    break;
                }
            }
            Collection[site.Url].RemoveAt(pluginIndex);
            PluginHost.Remove(site, name);
        }

        #endregion
    }
}