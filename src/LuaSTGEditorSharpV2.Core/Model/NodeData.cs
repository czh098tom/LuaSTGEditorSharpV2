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
        public static readonly NodeData Empty = new();

        private static readonly Predicate<NodeData> _default = _ => true;
        private static readonly Predicate<NodeData> _logical = n => n.IsActive;

        [JsonProperty]
        public string TypeUID { get; set; } = string.Empty;
        [JsonProperty]
        public bool IsActive { get; set; } = true;
        [JsonProperty]
        public Dictionary<string, string> Properties { get; private set; } = [];

        [JsonIgnore]
        public IReadOnlyList<NodeData> PhysicalChildren => _physicalChildren;

        [JsonProperty("Children")]
        private List<NodeData> _physicalChildren = [];
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
            foreach (var child in PhysicalChildren)
            {
                if (child.IsActive) yield return child;
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

        public void Replace(int position, NodeData node)
        {
            Remove(position);
            Insert(position, node);
        }

        public IEnumerable<NodeData> PerformBFS() => PerformBFS(_default);

        public IEnumerable<NodeData> PerformBFS(Predicate<NodeData> continuePred)
        {
            if (!continuePred(this)) yield break;
            Queue<NodeData> q = [];
            q.Enqueue(this);
            while (q.Count > 0)
            {
                var n = q.Dequeue();
                if (continuePred(n))
                {
                    foreach (var c in n._physicalChildren)
                    {
                        q.Enqueue(c);
                    }
                }
                yield return n;
            }
        }

        public IEnumerable<NodeData> PerformDFS() => PerformDFS(_default);

        public IEnumerable<NodeData> PerformDFS(Predicate<NodeData> continuePred)
        {
            if (!continuePred(this)) yield break;
            Stack<NodeData> q = [];
            q.Push(this);
            while (q.Count > 0)
            {
                var n = q.Pop();
                if (continuePred(n))
                {
                    foreach (var c in n._physicalChildren)
                    {
                        q.Push(c);
                    }
                }
                yield return n;
            }
        }

        public IReadOnlyList<NodeData> FindPhysicalMinForestContaining(IReadOnlyList<NodeData> nodes)
        {
            List<NodeData> result = [];
            HashSet<NodeData> resultSet = [];

            foreach (var node in nodes)
            {
                var toRemove = new List<NodeData>();
                foreach (var n in node.PerformBFS(n => !resultSet.Contains(n)))
                {
                    if (resultSet.Contains(n))
                    {
                        toRemove.Add(n);
                    }
                }
                foreach (var c in toRemove)
                {
                    result.Remove(c);
                    resultSet.Remove(c);
                }
                result.Add(node);
                resultSet.Add(node);
            }

            return result;
        }

        public NodeData DeepClone()
        {
            return JsonConvert.DeserializeObject<NodeData>(JsonConvert.SerializeObject(this))!;
        }
    }
}
