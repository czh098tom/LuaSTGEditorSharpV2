using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Configurable;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.ViewModel.Configurable
{
    public class SingleListViewModelProvider(ViewModelProviderServiceProvider nodeServiceProvider, IServiceProvider serviceProvider) 
        : ConfigurableViewModelProvider(nodeServiceProvider, serviceProvider)
    {
        [JsonProperty] public NodePropertyCapture? CountCapture { get; set; }
        [JsonProperty] public NodePropertyCapture?[]? SubCaptureRule { get; set; }
        [JsonProperty] public LocalizableArray<Selection>? SubCaptureFormat { get; set; }

        protected override int GetCaptureCacheLength()
        {
            if (CountCapture != null && SubCaptureRule != null && SubCaptureFormat != null)
            {
                var subCaptureFormat = SubCaptureFormat.GetLocalized();
                return base.GetCaptureCacheLength() + subCaptureFormat.Length;
            }
            return base.GetCaptureCacheLength();
        }

        protected override int WriteCaptureResult(NodeViewModelContext context, NodePropertyAccessToken token, string?[] captureResult)
        {
            int n = base.WriteCaptureResult(context, token, captureResult);
            if (CountCapture != null && SubCaptureRule != null && SubCaptureFormat != null)
            {
                var subCaptureFormat = SubCaptureFormat.GetLocalized();
                var countStr = CountCapture.Capture(token) ?? string.Empty;
                var subCaptureResult = new string[n + SubCaptureRule.Length];
                Array.Copy(captureResult, subCaptureResult, n);
                StringBuilder[] captureResultBuilder = new StringBuilder[subCaptureFormat.Length];
                for (int i = 0; i < subCaptureFormat.Length; i++)
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
                        for (int j = 0; j < subCaptureFormat.Length; j++)
                        {
                            var sel = subCaptureFormat[j];
                            if (sel.ShouldAppend(subCaptureResult))
                            {
                                captureResultBuilder[j].Append(context.Format(sel.Text, subCaptureResult));
                            }
                        }
                    }
                    for (int i = 0; i < subCaptureFormat.Length; i++)
                    {
                        captureResult[n + i] = captureResultBuilder[i].ToString();
                    }
                }
                n += subCaptureFormat.Length;
            }
            return n;
        }
    }
}
