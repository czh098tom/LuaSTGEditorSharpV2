using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter
{
    [JsonTypeShortName(typeof(ISharpNodeFormatConverter), "NodeTypeRemapping")]
    public class NodeTypeRemappingConverter : ISharpNodeFormatConverter
    {
        [JsonProperty] public string? To { get; private set; } = string.Empty;

        public NodeData Convert(NodeData source, SharpNodeFormattingContext context)
        {
#pragma warning disable CA2208
            if (string.IsNullOrEmpty(To))
            {
                throw new ArgumentNullException(nameof(To));
            }
#pragma warning restore CA2208
            source.TypeUID = To;
            return source;
        }
    }
}
