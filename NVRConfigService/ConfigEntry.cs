using System;
using System.IO;
using Microsoft.SharePoint;
using Navertica.SharePoint.Extensions;
using Navertica.SharePoint.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Navertica.SharePoint.ConfigService
{
    /// <summary>
    /// Stores data from a single NVR_SiteConfigJSON item, with no direct connection any to SPRequests.
    /// The JSON config data itself must be an object, names of attributes are "config branches",
    /// which can be custom classes with configurations inherited from ConfigBranch
    /// </summary>
    public class ConfigEntry
    {
        protected WebListItemId Ident;
        private Guid _itemId = Guid.Empty;

        public string JsonConfigBody;
        public string[] Apps, Urls, ContentTypes, ListTypes, ActiveForUsers;
        public bool Active, Approved;
        public int Order;

        private dynamic _branches;

        public Guid ItemId
        {
            get { return _itemId; }
        }

        private void InitFromDictionary(DictionaryNVR itemdict, SPWeb web)
        {
            if (itemdict == null || web == null) throw new ArgumentNullException();
            Urls = ((string)itemdict["NVR_SiteConfigUrl"] ?? "").Split('\n','|'); // HACK for newline not saving in field problem
            ContentTypes = ((string)itemdict["NVR_SiteConfigContentType"] ?? "").Split('\n');
            ListTypes = ((string)itemdict["NVR_SiteConfigListType"] ?? "").Split('\n');
            Apps = ((string) itemdict["NVR_SiteConfigApp"] ?? "").Split('\n', ',');   
            Order = int.Parse((itemdict["NVR_SiteConfigOrder"] ?? "0").ToString());
            JsonConfigBody = (string)itemdict["NVR_SiteConfigJSON"];
            Active = (bool)itemdict["NVR_SiteConfigActive"];
            Approved = (bool)itemdict["NVR_SiteConfigApproved"];

            if (!itemdict.ContainsKey("NVR_SiteConfigActiveFor") || itemdict["NVR_SiteConfigActiveFor"] == null || string.IsNullOrWhiteSpace((string)itemdict["NVR_SiteConfigActiveFor"]))
                ActiveForUsers = null;
            else
                ActiveForUsers = web.GetSPPrincipals((string)itemdict["NVR_SiteConfigActiveFor"]).GetLogins().ToArray();

            _itemId = (Guid)itemdict["_ItemUniqueId"];
            
            Deserialize();
        }

        /// <summary>
        /// Returns the JSON string of the value stored in the root JSON object under given name
        /// </summary>
        /// <param name="keyname"></param>
        /// <returns>JSON string of an object, or an object with a single attribute, "error"</returns>
        public string GetBranchJson(string keyname) 
        {
            if (string.IsNullOrWhiteSpace(keyname))
            {
                try
                {
                    return _branches.ToString();
                }
                catch (Exception)
                {
                    return string.Format("{{'error':'FAILED TO OPEN BRANCH {0} from json\n'}}", JsonConfigBody);
                }
            } 
            if (_branches == null && !string.IsNullOrEmpty(JsonConfigBody)) Deserialize();
            if (_branches == null) return string.Format("{{'error':'FAILED to load branch {0} from json\n{1}'}}", keyname, JsonConfigBody);
            try
            {
                return _branches[keyname].ToString();
            }
            catch (Exception)
            {
                return string.Format("{{'error':'FAILED TO OPEN BRANCH {0} from json\n{1}'}}", keyname, JsonConfigBody);
            }
        }
 
        private void Deserialize()
        {           
            if (!string.IsNullOrEmpty((JsonConfigBody ?? "").Trim()))
            {
                try
                {
                    if (JsonConfigBody.StartsWith("\"") && JsonConfigBody.EndsWith("\""))
                        _branches = JObject.Parse(JsonConfigBody.TrimStart(new[] { '"' }).TrimEnd(new[] { '"' }));
                    else
                        _branches = JObject.Parse(JsonConfigBody);
                }
                catch (Exception exc)
                {

                    var log = NaverticaLog.GetInstance();

                    log.LogError(exc, JsonConfigBody, Ident);
                }
            }
        }

        public ConfigEntry(DictionaryNVR itemdict, SPWeb web)
        {
            if (itemdict == null) throw new ArgumentNullException();
            if (web == null) throw new ArgumentNullException();
            if (itemdict.Count == 0) throw new ArgumentValidationException("itemdict", "empty");
            InitFromDictionary(itemdict, web);
        }

    }

}
