using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator
{
    [ServiceName("CodeGenerator"), ServiceShortName("cgen")]
    public class CodeGeneratorServiceBase 
        : NodeService<CodeGeneratorServiceBase, CodeGenerationContext, CodeGenerationServiceSettings>
    {
        private static readonly CodeGeneratorServiceBase _defaultService = new();

        static CodeGeneratorServiceBase()
        {
            _defaultServiceGetter = () => _defaultService;
        }

        public static IEnumerable<CodeData> ProceedWithIndention(NodeData node, CodeGenerationContext context, int indentionIncrement)
        {
            context.Push(node, indentionIncrement);
            foreach (NodeData child in node.GetLogicalChildren())
            {
                foreach (CodeData s in GetServiceOfNode(child).GenerateCodeWithContext(child, context))
                {
                    yield return s;
                }
            }
            context.Pop(indentionIncrement);
        }

        public static IEnumerable<CodeData> GenerateCode(NodeData nodeData, LocalSettings settings)
        {
            var ctx = GetContextOfNode(nodeData, settings);
            var service = GetServiceOfNode(nodeData);
            return service.GenerateCodeWithContext(nodeData, ctx);
        }

        public override sealed CodeGenerationContext GetEmptyContext(LocalSettings localSettings)
        {
            return new CodeGenerationContext(localSettings);
        }

        public virtual IEnumerable<CodeData> GenerateCodeWithContext(NodeData node, CodeGenerationContext context)
        {
            return ProceedWithIndention(node, context, 0);
        }
    }
}
