using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator
{
    /// <summary>
    /// Provide functionality of generating code from <see cref="NodeData"/>.
    /// </summary>
    [ServiceName("CodeGenerator"), ServiceShortName("cgen")]
    public class CodeGeneratorServiceBase 
        : NodeService<CodeGeneratorServiceBase, CodeGenerationContext, CodeGenerationServiceSettings>
    {
        private static readonly CodeGeneratorServiceBase _defaultService = new();

        static CodeGeneratorServiceBase()
        {
            _defaultServiceGetter = () => _defaultService;
        }

        /// <summary>
        /// Generate <see cref="CodeData"/> for the child of given node with 
        /// the context of the given node and indention increment.
        /// </summary>
        /// <param name="node"> The <see cref="NodeData"/>. </param>
        /// <param name="context"> The <see cref="CodeGenerationContext"/> of the node. </param>
        /// <param name="indentionIncrement"> The indention increment for its child. </param>
        /// <returns> <see cref="IEnumerable{T}"/> for enumerating <see cref="CodeData"/> generated. </returns>
        protected static IEnumerable<CodeData> GenerateForChildren(NodeData node, CodeGenerationContext context, int indentionIncrement)
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

        /// <summary>
        /// Generate <see cref="CodeData"/> for the given node.
        /// </summary>
        /// <param name="nodeData"> The <see cref="NodeData"/>. </param>
        /// <param name="settings"> The local params for executing the service. </param>
        /// <returns> <see cref="IEnumerable{T}"/> for enumerating <see cref="CodeData"/> generated. </returns>
        public static IEnumerable<CodeData> GenerateCode(NodeData nodeData, LocalParams settings)
        {
            var ctx = GetContextOfNode(nodeData, settings);
            var service = GetServiceOfNode(nodeData);
            return service.GenerateCodeWithContext(nodeData, ctx);
        }

        public override sealed CodeGenerationContext GetEmptyContext(LocalParams localSettings)
        {
            return new CodeGenerationContext(localSettings, ServiceSettings);
        }

        /// <summary>
        /// Generate <see cref="CodeData"/> for the given node with the same TypeUID.
        /// </summary>
        /// <param name="node"> The <see cref="NodeData"/>. </param>
        /// <param name="context"> The <see cref="CodeGenerationContext"/> of the node. </param>
        /// <returns> <see cref="IEnumerable{T}"/> for enumerating <see cref="CodeData"/> generated. </returns>
        protected virtual IEnumerable<CodeData> GenerateCodeWithContext(NodeData node, CodeGenerationContext context)
        {
            return GenerateForChildren(node, context, 0);
        }
    }
}
