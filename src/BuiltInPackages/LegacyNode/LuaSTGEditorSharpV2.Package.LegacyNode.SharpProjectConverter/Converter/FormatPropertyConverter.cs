using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Humanizer;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter
{
    [JsonTypeShortName(typeof(ISharpNodeFormatConverter), "FormatProperty")]
    public class FormatPropertyConverter : ISharpNodeFormatConverter
    {
        public NodeData Convert(NodeData source, SharpNodeFormattingContext context)
        {
            var prop = new Dictionary<string, string>();
            foreach (var kvp in source.Properties)
            {
                prop[kvp.Key
                    .ToLower()
                    .Replace("(", " ")
                    .Replace(")", "")
                    .Replace("'", "")
                    .Replace(",", " ")
                    .Underscore()] = kvp.Value;
            }
            source.Properties.Clear();
            foreach (var kvp in prop)
            {
                source.Properties.Add(kvp.Key, kvp.Value);
            }
            return source;
        }
    }
}
