using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator
{
    public class CaptureWithMacroOption : NodePropertyCapture
    {
        [JsonProperty] public bool DisableMacro { get; set; }

        public string ApplyMacro(NodeData node, CodeGenerationContext context)
        {
            string original = Capture(node) ?? string.Empty;
            return Apply(node, context, original);
        }

        public string ApplyMacroWithFormat(NodeData node, CodeGenerationContext context, object? arg0)
        {
            string original = CaptureByFormat(node, arg0) ?? string.Empty;
            return Apply(node, context, original);
        }

        public string ApplyMacroWithFormat(NodeData node, CodeGenerationContext context, params object?[] args)
        {
            string original = CaptureByFormat(node, args) ?? string.Empty;
            return Apply(node, context, original);
        }

        private string Apply(NodeData node, CodeGenerationContext context, string original)
        {
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
