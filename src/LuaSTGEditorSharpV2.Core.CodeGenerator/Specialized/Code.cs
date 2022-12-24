using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Specialized
{
    [Serializable]
    public class Code : CodeGeneratorServiceBase
    {
        public override IEnumerable<CodeData> GenerateCodeWithContext(NodeData node, CodeGenerationContext context)
        {
            StringBuilder sb = new();
            sb.AppendIndented(context.GetIndented(), node.GetProperty("code"));
            yield return new CodeData(sb.ToString(), node);
        }
    }
}
