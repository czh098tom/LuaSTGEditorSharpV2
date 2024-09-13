using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter
{
    [PackagePrimaryKey(nameof(SharpName))]
    public class SharpNodeFormatConverter(IServiceProvider serviceProvider)
        : PackedDataBase(serviceProvider)
    {
        [JsonProperty]
        public string? SharpName { get; private set; }

        [JsonProperty]
        public ISharpNodeFormatConverter?[]? Converters { get; private set; } = 
        [
            new StripNamespaceInTypeConverter(),
            new FormattedPropertyConverter(),
        ];

        public void Convert(NodeData source, SharpNodeFormattingContext context)
        {
            foreach (var conv in Converters ?? [])
            {
                var convResult = conv?.Convert(source, context) ?? source;
                source.PhysicalParent?.Replace(source.PhysicalParent.PhysicalChildren.FindIndex(source), convResult);
                source = convResult;
            }
            context.MakeParsed(source);
        }
    }
}
