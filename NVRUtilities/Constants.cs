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
namespace Navertica.SharePoint
{
    public static class Const
    {
        public const int LOG_EVENTID = 9999;
        public const string AREA_DEFAULT = "NAVERTICA";
        public const string SITECONFIG_LIST_TITLE = "SiteConfig";
        public const string SITESCRIPTS_LIST_TITLE = "SiteScripts";
        public const string SITESTRUCTURES_LIST_TITLE = "SiteStructures";
        public const string NVR_GROUP_BASE = "NAVERTICA";
        public const string NVR_GROUP_SYSTEM = "NAVERTICA core";

        public static string[] SiteConfigFields =
        {
            "SiteConfigApp",
            "SiteConfigUrl",
            "SiteConfigListType",
            "SiteConfigContentType",
            "SiteConfigJSON",
            "SiteConfigOrder",
            // ,SiteColumns.SiteConfigApproved
            // ,SiteColumns.SiteConfigTesters
            "SiteConfigDeleted"
        };
    }
}