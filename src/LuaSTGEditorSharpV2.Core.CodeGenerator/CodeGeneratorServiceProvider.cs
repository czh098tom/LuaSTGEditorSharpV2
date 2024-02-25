using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator
{
    [ServiceName("CodeGenerator"), ServiceShortName("cgen")]
    public class CodeGeneratorServiceProvider
        : CompactNodeServiceProvider<CodeGeneratorServiceProvider, CodeGeneratorServiceBase, CodeGenerationContext, CodeGenerationServiceSettings>
    {
        private static readonly CodeGeneratorServiceBase _defaultService = new();

        protected override CodeGeneratorServiceBase DefaultService => _defaultService;

        /// <summary>
        /// Generate <see cref="CodeData"/> for the child of given node with 
        /// the context of the given node and indention increment.
        /// </summary>
        /// <param name="node"> The <see cref="NodeData"/>. </param>
        /// <param name="context"> The <see cref="CodeGenerationContext"/> of the node. </param>
        /// <param name="indentionIncrement"> The indention increment for its child. </param>
        /// <returns> <see cref="IEnumerable{T}"/> for enumerating <see cref="CodeData"/> generated. </returns>
        public IEnumerable<CodeData> GenerateForChildren(NodeData node, CodeGenerationContext context, int indentionIncrement)
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

        public IEnumerable<CodeData> GenerateCode(NodeData nodeData, LocalServiceParam param)
            => GenerateCode(nodeData, param, ServiceSettings);

        /// <summary>
        /// Generate <see cref="CodeData"/> for the given node.
        /// </summary>
        /// <param name="nodeData"> The <see cref="NodeData"/>. </param>
        /// <param name="param"> The local params for executing the service. </param>
        /// <param name="serviceSettings"> The <see cref="CodeGenerationServiceSettings"/> of this action. </param>
        /// <returns> <see cref="IEnumerable{T}"/> for enumerating <see cref="CodeData"/> generated. </returns>
        public IEnumerable<CodeData> GenerateCode(NodeData nodeData, LocalServiceParam param
            , CodeGenerationServiceSettings serviceSettings)
        {
            var ctx = GetContextOfNode(nodeData, param, serviceSettings);
            var service = GetServiceOfNode(nodeData);
            return service.GenerateCodeWithContext(nodeData, ctx);
        }

        public LanguageBase? GetLanguageOfNode(NodeData nodeData)
        {
            return HostedApplicationHelper.GetService<LanguageProviderService>()
                .GetLanguage(GetServiceOfNode(nodeData).Language);
        }
    }
}
