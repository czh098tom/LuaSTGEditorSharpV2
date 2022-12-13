using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable
{
    [Serializable]
    public record class ContextCapture
    {
        [JsonProperty] public string TypeUID { get; private set; }
        [JsonProperty] public string[] Property { get; private set; }

        public ContextCapture(string typeUID, string[] property)
        {
            TypeUID = typeUID;
            Property = property;
        }
    }
}
