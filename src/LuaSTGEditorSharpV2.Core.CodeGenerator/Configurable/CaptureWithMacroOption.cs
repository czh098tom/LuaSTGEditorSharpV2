using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable
{
    public record CaptureWithMacroOption(string Capture, bool DisableMacro)
    {
        public string ApplyMacro(NodeData node, CodeGenerationContext context)
        {
            string original = node.GetProperty(Capture);
            if (!DisableMacro)
            {
                return context.ApplyMacro(node, original);
            }
            else
            {
                return original;
            }
        }
    }
}
