using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter
{
    public class StripNamespaceInTypeConverter : ISharpNodeFormatConverter
    {
        public NodeData Convert(NodeData source, SharpNodeFormattingContext context)
        {
            source.TypeUID = source.TypeUID.Split(',')[0].Split('.')[^1];
            return source;
        }
    }
}
