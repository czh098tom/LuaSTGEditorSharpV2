using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Services
{
    public class ActiveDocumentServiceSettings
    {
        [JsonProperty]
        public string CustomizedUntitledName { get; set; } = string.Empty;
    }
}
