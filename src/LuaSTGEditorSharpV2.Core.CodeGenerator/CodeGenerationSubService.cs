using System;
using System.Collections.Generic;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator
{
    public abstract class CodeGenerationSubService(
        CodeGeneratorServiceProvider nodeServiceProvider,
        IServiceProvider serviceProvider)
        : CodeGeneratorServiceBase(nodeServiceProvider, serviceProvider)
    {
        protected internal override IEnumerable<CodeData> GenerateCodeWithContext(
            NodeData node, CodeGenerationContext context)
        {
            return [];
        }
    }
}
