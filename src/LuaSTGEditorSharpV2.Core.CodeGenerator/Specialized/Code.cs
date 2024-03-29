﻿using System;
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
        internal protected override IEnumerable<CodeData> GenerateCodeWithContext(NodeData node, CodeGenerationContext context)
        {
            StringBuilder sb = context.ApplyIndented(context.GetIndented()
                , context.ApplyMacro(node, node.GetProperty("code")));
            yield return new CodeData(sb.ToString(), node);
        }
    }
}
