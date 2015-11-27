using Navertica.SharePoint.ConfigService;
using Navertica.SharePoint.RepoService;

namespace Navertica.SharePoint.PageHead
{
    public interface IPageHead
    {
        string Category();
        object LoadContents(ConfigServiceClient cfg, RepoServiceClient repo);
        ConfigGuidList GetConfigGuidList(ConfigServiceClient cfg);
    }
}