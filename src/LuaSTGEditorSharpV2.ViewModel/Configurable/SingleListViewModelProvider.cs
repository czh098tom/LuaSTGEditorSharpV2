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
                return base.GetCaptureCacheLength() + 1;
            }
            return base.GetCaptureCacheLength();
        }

        protected override int WriteCaptureResult(NodePropertyAccessToken token, string?[] captureResult)
        {
            int n = base.WriteCaptureResult(token, captureResult);
            if (CountCapture != null && SubCaptureRule != null && SubCaptureFormat != null)
            {
                var countStr = CountCapture.Capture(token) ?? string.Empty;
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
                            subCaptureResult[j] = SubCaptureRule[j]?.CaptureByFormat(token, idx) ?? string.Empty;
                        }
                        var subCaptureFormat = SubCaptureFormat.GetLocalized();
                        for (int j = 0; j < subCaptureFormat.Length; j++)
                        {
                            var sel = subCaptureFormat[j];
                            if (sel.ShouldAppend(subCaptureResult))
                            {
                                sb.AppendFormat(sel.Text, subCaptureResult);
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
