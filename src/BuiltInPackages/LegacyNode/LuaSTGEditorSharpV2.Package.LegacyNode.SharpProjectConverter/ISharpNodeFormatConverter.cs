using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter
{
    public interface ISharpNodeFormatConverter
    {
        NodeData Convert(NodeData source, SharpNodeFormattingContext context);
    }
}
