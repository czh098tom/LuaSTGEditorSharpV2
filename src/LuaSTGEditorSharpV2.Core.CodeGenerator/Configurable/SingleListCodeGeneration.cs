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
    public class SingleListCodeGeneration(CodeGeneratorServiceProvider nodeServiceProvider, IServiceProvider serviceProvider) : ConfigurableCodeGeneration(nodeServiceProvider, serviceProvider)
    {
        [JsonProperty] public NodePropertyCapture? CountCapture { get; set; }
        [JsonProperty] public CaptureWithMacroOption?[]? SubCaptureRule { get; set; }
        [JsonProperty] public Selection[]? SubCaptureFormat { get; set; }

        protected override int GetCaptureCacheLength()
        {
            if (CountCapture != null && SubCaptureRule != null && SubCaptureFormat != null)
            {
                return base.GetCaptureCacheLength() + SubCaptureFormat.Length;
            }
            return base.GetCaptureCacheLength();
        }

        protected override int WriteCaptureResult(string?[] captureResult, NodeData node, CodeGenerationContext context)
        {
            var token = new NodePropertyAccessToken(ServiceProvider, node, context);
            int n = base.WriteCaptureResult(captureResult, node, context);
            if (CountCapture != null && SubCaptureRule != null && SubCaptureFormat != null)
            {
                var countStr = CountCapture.Capture(token) ?? string.Empty;
                var subCaptureResult = new string[n + SubCaptureRule.Length];
                StringBuilder[] captureResultBuilder = new StringBuilder[SubCaptureFormat.Length];
                Array.Copy(captureResult, subCaptureResult, n);
                for (int i = 0; i < SubCaptureFormat.Length; i++)
                {
                    captureResult[n + i] = string.Empty;
                    captureResultBuilder[i] = new StringBuilder();
                }
                if (int.TryParse(countStr, out var count))
                {
                    for (int i = 0; i < count; i++)
                    {
                        object idx = i;
                        for (int j = 0; j < SubCaptureRule.Length; j++)
                        {
                            subCaptureResult[n + j] = SubCaptureRule[j]?.CaptureByFormat(token, idx) ?? string.Empty;
                        }
                        for (int j = 0; j < SubCaptureFormat.Length; j++)
                        {
                            var sel = SubCaptureFormat[j];
                            if (sel.ShouldAppend(subCaptureResult))
                            {
                                captureResultBuilder[j].Append(context.ApplyIndentedFormat(sel.Text, subCaptureResult));
                            }
                        }
                    }
                    for (int i = 0; i < SubCaptureFormat.Length; i++)
                    {
                        captureResult[n + i] = captureResultBuilder[i].ToString();
                    }
                }
                n += SubCaptureFormat.Length;
            }
            return n;
        }
    }
}
