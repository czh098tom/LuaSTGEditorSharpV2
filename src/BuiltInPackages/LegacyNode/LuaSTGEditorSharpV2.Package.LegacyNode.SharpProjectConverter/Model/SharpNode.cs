using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Model
{
    public class SharpNode
    {
        [JsonProperty("$type")]
        public string? Type { get; set; } = string.Empty;
        [JsonProperty("Attributes")]
        public SharpAttribute?[]? SharpAttributes { get; set; } = [];
        [JsonProperty("IsBanned")]
        public bool IsBanned { get; set; }

        public NodeData ToNodeData()
        {
            var n = new NodeData(Type ?? string.Empty);
            foreach (var attr in SharpAttributes ?? [])
            {
                if (attr != null)
                {
                    n.Properties.Add(attr.AttrCap ?? string.Empty, attr.AttrInput ?? string.Empty);
                }
            }
            n.IsActive = !IsBanned;
            return n;
        }
    }

    public class SharpAttribute
    {
        [JsonProperty("attrCap")]
        public string? AttrCap { get; set; } = string.Empty;
        [JsonProperty("attrInput")]
        public string? AttrInput { get; set; } = string.Empty;
    }

}
