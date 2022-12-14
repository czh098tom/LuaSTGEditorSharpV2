using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Core.Model
{
    [Serializable]
    public class NodeData
    {
        [JsonProperty] public string TypeUID { get; private set; } = string.Empty;
        [JsonProperty] public Dictionary<string, string> Properties { get; private set; } = new Dictionary<string, string>();
        [JsonProperty("Children")] public List<NodeData> PhysicalChildren { get; private set; } = new List<NodeData>();
        [JsonProperty] public bool IsActive { get; set; } = true;

        [JsonIgnore] public NodeData? PhysicalParent { get; private set; } = null;
    }
}
