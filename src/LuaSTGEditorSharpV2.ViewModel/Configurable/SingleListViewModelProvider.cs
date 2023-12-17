using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Configurable;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.ViewModel.Configurable
{
    public class SingleListViewModelProvider : ConfigurableViewModelProvider
    {
        [JsonProperty] public string? CountCapture { get; set; }
        [JsonProperty] public string[]? SubCaptureRule { get; set; }
        [JsonProperty] public LocalizableArray<Selection>? SubCaptureFormat { get; set; }

        protected override int GetCaptureCacheLength()
        {
            if (CountCapture != null && SubCaptureRule != null && SubCaptureFormat != null)
            {
                return base.GetCaptureCacheLength() + 1;
            }
            return base.GetCaptureCacheLength();
        }

        protected override int WriteCaptureResult(NodeData dataSource, string?[] captureResult)
        {
            int n = base.WriteCaptureResult(dataSource, captureResult);
            if (CountCapture != null && SubCaptureRule != null && SubCaptureFormat != null)
            {
                var countStr = dataSource.GetProperty(CountCapture);
                captureResult[n] = string.Empty;
                var subCaptureResult = new string[SubCaptureRule.Length];
                if (int.TryParse(countStr, out var count))
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < count; i++)
                    {
                        for (int j = 0; j < SubCaptureRule.Length; j++)
                        {
                            subCaptureResult[j] = dataSource.GetProperty(string.Format(SubCaptureRule[j], i));
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
