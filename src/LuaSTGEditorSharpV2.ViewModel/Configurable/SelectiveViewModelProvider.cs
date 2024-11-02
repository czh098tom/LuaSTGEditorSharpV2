using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Configurable;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.ViewModel.Configurable
{
    [Serializable]
    public class SelectiveViewModelProvider(ViewModelProviderServiceProvider nodeServiceProvider, IServiceProvider serviceProvider)
        : ViewModelProviderServiceBase(nodeServiceProvider, serviceProvider)
    {
        [JsonProperty] public NodePropertyCapture?[] Captures { get; private set; } = [];
        [JsonProperty] public string Icon { get; private set; } = "";
        [JsonProperty] public LocalizableArray<Selection>? Text { get; private set; }

        private string[]? _captureResult;

        internal protected override void UpdateViewModelData(NodeViewModel viewModel, NodeData dataSource, NodeViewModelContext context)
        {
            var token = new NodePropertyAccessToken(ServiceProvider, dataSource, context);
            _captureResult ??= new string[GetCaptureCacheLength()];
            int n;
            for (n = 0; n < Captures.Length; n++)
            {
                _captureResult[n] = Captures[n]?.Capture(token) ?? string.Empty;
            }
            Selection[] text = Text?.GetLocalized() ?? [];
            StringBuilder sb = new();
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i].ShouldAppend(_captureResult))
                {
                    sb.Append(context.Format(text[i].Text ?? string.Empty, _captureResult));
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
