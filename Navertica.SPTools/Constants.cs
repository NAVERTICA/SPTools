namespace Navertica.SharePoint
{
    public static class Const
    {
        public const int LOG_EVENTID = 9999;
        public const string AREA_DEFAULT = "NAVERTICA";
        public const string SITECONFIG_LIST_TITLE = "SiteConfig";
        public const string SITESCRIPTS_LIST_TITLE = "SiteScripts";
        public const string SITESTRUCTURES_LIST_TITLE = "SiteStructures";
        public const string DEFAULT_GROUP = "NAVERTICA";
        public const string NVR_GROUP_BASE = "NAVERTICA";
        public const string NVR_GROUP_SYSTEM = "NAVERTICA core";
        public const string NVR_DLL = "Navertica.SPTools, Version=0.8.0.0, Culture=neutral, PublicKeyToken=7a3c1eb1e2411690";
        public const string NVR_LIST_RECEIVER = "Navertica.SharePoint.Receivers.ListReceiver";

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