using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter
{
    [JsonTypeShortName(typeof(ISharpNodeFormatConverter), "Default")]
    public class DefaultSharpNodeFormatConverter : CompositeSharpNodeFormatConverter
    {
        public DefaultSharpNodeFormatConverter() 
        {
            Converters =
            [
                new StripNamespaceInTypeConverter(),
                new FormatPropertyConverter(),
            ];
        }
    }
}
