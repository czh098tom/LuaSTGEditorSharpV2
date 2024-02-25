using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator
{
    /// <summary>
    /// Provide functionality of generating code from <see cref="NodeData"/>.
    /// </summary>
    public class CodeGeneratorServiceBase 
        : CompactNodeService<CodeGeneratorServiceProvider, CodeGeneratorServiceBase, CodeGenerationContext, CodeGenerationServiceSettings>
    {
        [JsonProperty("Language")]
        public string Language { get; private set; } = string.Empty;

        public override sealed CodeGenerationContext GetEmptyContext(LocalServiceParam localSettings
            , CodeGenerationServiceSettings serviceSettings)
        {
            return new CodeGenerationContext(localSettings, serviceSettings);
        }

        /// <summary>
        /// Generate <see cref="CodeData"/> for the given node with the same TypeUID.
        /// </summary>
        /// <param name="node"> The <see cref="NodeData"/>. </param>
        /// <param name="context"> The <see cref="CodeGenerationContext"/> of the node. </param>
        /// <returns> <see cref="IEnumerable{T}"/> for enumerating <see cref="CodeData"/> generated. </returns>
        internal protected virtual IEnumerable<CodeData> GenerateCodeWithContext(NodeData node, CodeGenerationContext context)
        {
            return GetServiceProvider().GenerateForChildren(node, context, 0);
        }
    }
}
