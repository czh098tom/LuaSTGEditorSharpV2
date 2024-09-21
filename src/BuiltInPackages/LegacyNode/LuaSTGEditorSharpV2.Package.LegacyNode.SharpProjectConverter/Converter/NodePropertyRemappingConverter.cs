using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter
{
    [JsonTypeShortName(typeof(ISharpNodeFormatConverter), "NodePropertyRemapping")]
    public class NodePropertyRemappingConverter : ISharpNodeFormatConverter
    {
        [JsonProperty] public string? OriginalKey { get; private set; } = string.Empty;
        [JsonProperty] public string? NewKey { get; private set; } = string.Empty;

        public NodeData Convert(NodeData source, SharpNodeFormattingContext context)
        {
#pragma warning disable CA2208
            if (string.IsNullOrEmpty(OriginalKey))
            {
                throw new ArgumentNullException(nameof(OriginalKey));
            }
            if (string.IsNullOrEmpty(NewKey))
            {
                throw new ArgumentNullException(nameof(NewKey));
            }
#pragma warning restore CA2208
            var originalValue = source.Properties.GetValueOrDefault(OriginalKey, string.Empty);
            if (source.Properties.Remove(OriginalKey))
            {
                source.Properties[NewKey] = originalValue;
            }
            return source;
        }
    }
}
