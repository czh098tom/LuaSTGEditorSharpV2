using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Model;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter
{
    public class SharpNodeSerializer
    {
        public static readonly JsonSerializerSettings TreeNodeSettings =
            new()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                SerializationBinder = new SharpTypeBinder()
            };

        public NodeData? DeserializeTreeNode(string s)
        {
            return JsonConvert.DeserializeObject<SharpNode>(s, TreeNodeSettings)?.ToNodeData();
        }
    }
}
