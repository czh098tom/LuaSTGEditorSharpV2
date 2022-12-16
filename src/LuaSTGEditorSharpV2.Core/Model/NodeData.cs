using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Core.Model
{
    [Serializable]
    public class NodeData
    {
        [JsonProperty] 
        public string TypeUID { get; private set; } = string.Empty;
        [JsonProperty] 
        public Dictionary<string, string> Properties { get; private set; } = new Dictionary<string, string>();
        [JsonProperty]
        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        public IReadOnlyList<NodeData> PhysicalChildren =>_physicalChildren;

        [JsonProperty("Children")]
        private List<NodeData> _physicalChildren = new List<NodeData>();
        [JsonIgnore] public NodeData? PhysicalParent { get; private set; } = null;

        [OnDeserialized]
        public void GenerateParentForChildren(StreamingContext context)
        {
            for (int i = 0; i < PhysicalChildren.Count; i++)
            {
                PhysicalChildren[i].PhysicalParent = this;
            }
        }

        public void Insert(int position, NodeData node)
        {
            _physicalChildren.Insert(position, node);
            node.PhysicalParent = this;
        }

        public NodeData Remove(int position)
        {
            var n = _physicalChildren[position];
            n.PhysicalParent = null;
            _physicalChildren.RemoveAt(position);
            return n;
        }
    }
}
