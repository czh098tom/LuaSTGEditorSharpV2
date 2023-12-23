using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using System.Globalization;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.ViewModel.Specialized
{
    [Serializable]
    internal class ServiceDefinition : ViewModelProviderServiceBase
    {
        [JsonProperty] public string Icon { get; private set; } = "";
        [JsonProperty] public NodePropertyCapture? DeclarationCaputure { get; private set; }
        [JsonProperty] public NodePropertyCapture? ShortNameCaputure { get; private set; }
        [JsonProperty] public LocalizableString? Text { get; private set; }
        [JsonProperty] public LocalizableString? ErrorText { get; private set; }

        internal protected override void UpdateViewModelData(NodeViewModel viewModel, NodeData dataSource, NodeViewModelContext context)
        {
            var shortName = ShortNameCaputure?.Capture(dataSource) ?? string.Empty;
            var jsonDecl = DeclarationCaputure?.Capture(dataSource) ?? string.Empty;
            try
            {
                var nodePackageProvider = HostedApplicationHelper.GetService<NodePackageProvider>();
                var type = nodePackageProvider.GetServiceTypeOfShortName(shortName);
                var obj = JsonConvert.DeserializeObject(jsonDecl, type);
                string? uid = type.BaseType?.GetProperty("TypeUID")?.GetValue(obj) as string;
                viewModel.Text = string.Format(Text?.GetLocalized() ?? string.Empty
                    , shortName, uid ?? throw new NullReferenceException());
            }
            catch
            {
                viewModel.Text = string.Format(ErrorText?.GetLocalized() ?? string.Empty
                    , shortName);
            }
            viewModel.Icon = Icon;
        }
    }
}
