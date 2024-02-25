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

        public string ApplyMacro(NodePropertyAccessToken token, CodeGenerationContext context)
        {
            string original = Capture(token) ?? string.Empty;
            return Apply(token.NodeData, context, original);
        }

        public string ApplyMacroWithFormat(NodePropertyAccessToken token, CodeGenerationContext context, object? arg0)
        {
            string original = CaptureByFormat(token, arg0) ?? string.Empty;
            return Apply(token.NodeData, context, original);
        }

        public string ApplyMacroWithFormat(NodePropertyAccessToken token, CodeGenerationContext context, params object?[] args)
        {
            string original = CaptureByFormat(token, args) ?? string.Empty;
            return Apply(token.NodeData, context, original);
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
