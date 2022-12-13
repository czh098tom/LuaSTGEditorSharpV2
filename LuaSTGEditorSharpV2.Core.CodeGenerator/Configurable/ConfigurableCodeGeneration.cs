using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using System.Runtime.Serialization;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable
{
    [Serializable]
    public class ConfigurableCodeGeneration : CodeGeneratorServiceBase
    {
        [JsonProperty] public string[] Captures { get; private set; } = Array.Empty<string>();
        [JsonProperty] public ContextCapture[] ContextCaptures { get; private set; } = Array.Empty<ContextCapture>();
        [JsonProperty] public string? Head { get; private set; }
        [JsonProperty] public string? Tail { get; private set; }
        [JsonProperty] public bool IgnoreChildren { get; private set; } = false;
        [JsonProperty] public int IndentionIncrement { get; private set; } = 1;

        private string?[]? _captureResult;

        public override IEnumerable<CodeData> GenerateCodeWithContext(NodeData node, CodeGenerationContext context)
        {
            _captureResult ??= new string[GetCaptureCacheLength()];
            int n;
            for (n = 0; n < Captures.Length; n++)
            {
                _captureResult[n] = node.Properties[Captures[n]];
            }
            for (int i = 0; i < ContextCaptures.Length; i++)
            {
                for (int j = 0; j < ContextCaptures[i].Property.Length; j++)
                {
                    _captureResult[n] = context.PeekType(ContextCaptures[i].TypeUID)?.Properties[ContextCaptures[i].Property[j]];
                    n++;
                }
            }
            if (Head != null) yield return new CodeData(context.GetIndented().AppendFormat(Head, _captureResult).ToString(), node);
            if (!IgnoreChildren)
            {
                foreach (var cd in ProceedWithIndention(node, context, IndentionIncrement))
                {
                    yield return cd;
                }
            }
            if (Tail != null) yield return new CodeData(context.GetIndented().AppendFormat(Tail, _captureResult).ToString(), node);
        }

        private int GetCaptureCacheLength()
        {
            int l = Captures.Length;
            for (int i = 0; i < ContextCaptures.Length; i++)
            {
                l += ContextCaptures[i].Property.Length;
            }
            return l;
        }
    }
}
