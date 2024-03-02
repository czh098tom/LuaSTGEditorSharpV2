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
    public class SingleListViewModelProvider : ConfigurableViewModelProvider
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

        protected override int WriteCaptureResult(NodePropertyAccessToken token, string?[] captureResult)
        {
            int n = base.WriteCaptureResult(token, captureResult);
            if (CountCapture != null && SubCaptureRule != null && SubCaptureFormat != null)
            {
                var subCaptureFormat = SubCaptureFormat.GetLocalized();
                var countStr = CountCapture.Capture(token) ?? string.Empty;
                var subCaptureResult = new string[SubCaptureRule.Length];
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
                            subCaptureResult[j] = SubCaptureRule[j]?.CaptureByFormat(token, idx) ?? string.Empty;
                        }
                        for (int j = 0; j < subCaptureFormat.Length; j++)
                        {
                            var sel = subCaptureFormat[j];
                            if (sel.ShouldAppend(subCaptureResult))
                            {
                                captureResultBuilder[j].AppendFormat(sel.Text, subCaptureResult);
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
