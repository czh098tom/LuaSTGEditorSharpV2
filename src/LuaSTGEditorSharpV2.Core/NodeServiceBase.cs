using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Core
{
    [PackagePrimaryKey(nameof(TypeUID))]
    public class NodeServiceBase
    {
        [JsonProperty]
        public string TypeUID { get; protected set; } = string.Empty;
    }
}
