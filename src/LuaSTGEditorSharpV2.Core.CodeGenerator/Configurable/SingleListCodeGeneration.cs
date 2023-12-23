using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Configurable;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable
{
    public class SingleListCodeGeneration : ConfigurableCodeGeneration
    {
        [JsonProperty] public NodePropertyCapture? CountCapture { get; set; }
        [JsonProperty] public CaptureWithMacroOption?[]? SubCaptureRule { get; set; }
        [JsonProperty] public Selection[]? SubCaptureFormat { get; set; }

        protected override int GetCaptureCacheLength()
        {
            if (CountCapture != null && SubCaptureRule != null && SubCaptureFormat != null)
            {
                return base.GetCaptureCacheLength() + 1;
            }
            return base.GetCaptureCacheLength();
        }

        protected override int WriteCaptureResult(string?[] captureResult, NodeData node, CodeGenerationContext context)
        {
            int n = base.WriteCaptureResult(captureResult, node, context);
            if (CountCapture != null && SubCaptureRule != null && SubCaptureFormat != null)
            {
                var countStr = CountCapture.Capture(node) ?? string.Empty;
                captureResult[n] = string.Empty;
                var subCaptureResult = new string[SubCaptureRule.Length];
                if (int.TryParse(countStr, out var count))
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < count; i++)
                    {
                        object idx = i;
                        for (int j = 0; j < SubCaptureRule.Length; j++)
                        {
                            subCaptureResult[j] = SubCaptureRule[j]?.CaptureByFormat(node, idx) ?? string.Empty;
                        }
                        for (int j = 0; j < SubCaptureFormat.Length; j++)
                        {
                            var sel = SubCaptureFormat[j];
                            if (sel.ShouldAppend(subCaptureResult))
                            {
                                sb.Append(context.ApplyIndentedFormat(context.GetIndented(), sel.Text, subCaptureResult));
                            }
                        }
                    }
                    captureResult[n] = sb.ToString();
                }
                n++;
            }
            return n;
        }
    }
}
