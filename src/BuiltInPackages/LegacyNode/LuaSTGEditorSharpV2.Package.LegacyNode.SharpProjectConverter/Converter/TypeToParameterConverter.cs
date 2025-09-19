using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter
{
    /// <summary>
    /// 合并到目标类型所需的额外参数
    /// </summary>
    [JsonTypeShortName(typeof(ISharpNodeFormatConverter), "TypeToParameter")]
    public class TypeToParameterConverter : ISharpNodeFormatConverter
    {
        [JsonProperty] public Dictionary<string, string>? DefinedParameters { get; private set; } = [];

        public NodeData Convert(NodeData source, SharpNodeFormattingContext context)
        {
            if (DefinedParameters == null)
            {
                return source;
            }
            foreach (var kvp in DefinedParameters)
            {
                source.Properties[kvp.Key] = kvp.Value;
            }

            return source;
        }
    }
}