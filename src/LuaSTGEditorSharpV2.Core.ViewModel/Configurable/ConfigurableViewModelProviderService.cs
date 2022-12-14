using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using System.Xml.Linq;

namespace LuaSTGEditorSharpV2.Core.ViewModel.Configurable
{
    [Serializable]
    public class ConfigurableViewModelProviderService : ViewModelProviderService
    {
        [JsonProperty] public string[] Captures { get; private set; } = Array.Empty<string>();
        [JsonProperty] public string Icon { get; private set; } = "";
        [JsonProperty] public string Text { get; private set; } = "";

        private string?[]? _captureResult;

        protected override void UpdateViewModel(NodeViewModel viewModel, NodeData dataSource)
        {
            _captureResult ??= new string[GetCaptureCacheLength()];
            int n;
            for (n = 0; n < Captures.Length; n++)
            {
                _captureResult[n] = dataSource.Properties[Captures[n]];
            }
            viewModel.Text = string.Format(Text, _captureResult);
            viewModel.Icon = Icon;
        }

        private int GetCaptureCacheLength()
        {
            int l = Captures.Length;
            return l;
        }
    }
}
