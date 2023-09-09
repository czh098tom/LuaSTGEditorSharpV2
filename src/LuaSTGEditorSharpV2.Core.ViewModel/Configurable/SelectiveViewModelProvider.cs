using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.ViewModel.Configurable
{
    [Serializable]
    public class SelectiveViewModelProvider
    {
        [Serializable]
        public class ConfigurableViewModelProvider : ViewModelProviderServiceBase
        {
            [JsonProperty] public string[] Captures { get; private set; } = Array.Empty<string>();
            [JsonProperty] public string Icon { get; private set; } = "";
            [JsonProperty] public ViewModelTextSelection[] Text { get; private set; } = Array.Empty<ViewModelTextSelection>();
            [JsonProperty] public Dictionary<string, ViewModelTextSelection[]> LocalizedText { get; private set; } = new();

            private string?[]? _captureResult;

            protected override void UpdateViewModelData(NodeViewModel viewModel, NodeData dataSource, NodeViewModelContext context)
            {
                _captureResult ??= new string[GetCaptureCacheLength()];
                int n;
                for (n = 0; n < Captures.Length; n++)
                {
                    _captureResult[n] = dataSource.GetProperty(Captures[n]);
                }
                ViewModelTextSelection[] text = LocalizedText
                    .GetValueOrDefault(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, Text);
                StringBuilder sb = new();
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i].ShouldAppend(dataSource))
                    {
                        sb.AppendFormat(text[i].Text, _captureResult);
                    }
                }
                viewModel.Text = sb.ToString();
                viewModel.Icon = Icon;
            }

            private int GetCaptureCacheLength()
            {
                int l = Captures.Length;
                return l;
            }
        }
    }
}
