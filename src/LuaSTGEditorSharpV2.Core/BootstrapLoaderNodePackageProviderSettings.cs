using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Core
{
    public class BootstrapLoaderNodePackageProviderSettings
    {
        [JsonProperty]
        public List<string> Packages = [ "Core" ];
    }
}
