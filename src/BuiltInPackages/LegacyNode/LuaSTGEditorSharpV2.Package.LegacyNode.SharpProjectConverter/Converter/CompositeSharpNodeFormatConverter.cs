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
    [JsonTypeShortName(typeof(ISharpNodeFormatConverter), "Composite")]
    public class CompositeSharpNodeFormatConverter : ISharpNodeFormatConverter
    {
        [JsonProperty]
        public ISharpNodeFormatConverter?[]? Converters { get; set; }

        public NodeData Convert(NodeData source, SharpNodeFormattingContext context)
        {
            if (Converters != null)
            {
                foreach (var converter in Converters)
                {
                    source = converter?.Convert(source, context) ?? source;
                }
            }
            return source;
        }
    }
}
