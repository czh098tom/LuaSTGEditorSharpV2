﻿using System;
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
        public static readonly NodeData Empty = new();

        [JsonProperty]
        public string TypeUID { get; private set; } = string.Empty;
        [JsonProperty]
        public bool IsActive { get; set; } = true;
        [JsonProperty]
        public Dictionary<string, string> Properties { get; private set; } = [];

        [JsonIgnore]
        public IReadOnlyList<NodeData> PhysicalChildren => _physicalChildren;

        [JsonProperty("Children")]
        private List<NodeData> _physicalChildren = new();
        [JsonIgnore] public NodeData? PhysicalParent { get; private set; } = null;

        [OnDeserialized]
        public void GenerateParentForChildren(StreamingContext context)
        {
            for (int i = 0; i < PhysicalChildren.Count; i++)
            {
                PhysicalChildren[i].PhysicalParent = this;
            }
        }

        public string GetProperty(string key, string @default = "") => Properties.GetValueOrDefault(key, @default);

        public bool HasProperty(string key) => Properties.ContainsKey(key);

        public NodeData() { }

        public NodeData(string typeUID) 
        {
            TypeUID = typeUID;
        }

        public IEnumerable<NodeData> GetLogicalChildren()
        {
            foreach(var child in PhysicalChildren)
            {
                if(child.IsActive) yield return child;
            }
        }

        public void Add(NodeData node)
        {
            _physicalChildren.Add(node);
            node.PhysicalParent = this;
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
