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