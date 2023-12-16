using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.ViewModel.Configurable
{
    [Serializable]
    public class ConfigurableViewModelProvider : ViewModelProviderServiceBase
    {
        [JsonProperty] public string[] Captures { get; private set; } = Array.Empty<string>();
        [JsonProperty] public string Icon { get; private set; } = "";
        [JsonProperty] public LocalizableString? Text { get; private set; }

        private string?[]? _captureResult;

        internal protected override void UpdateViewModelData(NodeViewModel viewModel, NodeData dataSource, NodeViewModelContext context)
        {
            _captureResult ??= new string[GetCaptureCacheLength()];
            int n;
            for (n = 0; n < Captures.Length; n++)
            {
                _captureResult[n] = dataSource.GetProperty(Captures[n]);
            }
            string text = GetLocalizedTextIfExists();
            viewModel.Text = string.Format(text, _captureResult);
            viewModel.Icon = Icon;
        }

        private string GetLocalizedTextIfExists()
        {
            return Text?.GetLocalized() ?? string.Empty;
        }

        private int GetCaptureCacheLength()
        {
            int l = Captures.Length;
            return l;
        }
    }
}
