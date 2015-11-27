using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Navertica.SharePoint;

namespace Navertica.SharePoint.RepoService
{
    /// <summary>
    /// Basically a Dictionary&lt;string, object&gt; for storing variables inbetween scripts
    /// </summary>
    public class ScriptContext : DictionaryNVR
    {
        public ScriptContext(IEnumerable<KeyValuePair<string, object>> d)
        {
            foreach (KeyValuePair<string, object> kvp in d)
            {
                this[kvp.Key] = kvp.Value;
            }
        }

        public ScriptContext()
        {
        }
    };
}
