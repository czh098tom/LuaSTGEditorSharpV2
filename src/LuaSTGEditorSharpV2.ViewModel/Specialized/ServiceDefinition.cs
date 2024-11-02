using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using System.Globalization;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.ViewModel.Specialized
{
    [Serializable]
    internal class ServiceDefinition(ViewModelProviderServiceProvider nodeServiceProvider, IServiceProvider serviceProvider) 
        : ViewModelProviderServiceBase(nodeServiceProvider, serviceProvider)
    {
        [JsonProperty] public string Icon { get; private set; } = "";
        [JsonProperty] public NodePropertyCapture? DeclarationCaputure { get; private set; }
        [JsonProperty] public NodePropertyCapture? ShortNameCaputure { get; private set; }
        [JsonProperty] public LocalizableString? Text { get; private set; }
        [JsonProperty] public LocalizableString? ErrorText { get; private set; }

        internal protected override void UpdateViewModelData(NodeViewModel viewModel, NodeData dataSource, NodeViewModelContext context)
        {
            var token = new NodePropertyAccessToken(ServiceProvider, dataSource, context);
            var shortName = ShortNameCaputure?.Capture(token) ?? string.Empty;
            var jsonDecl = DeclarationCaputure?.Capture(token) ?? string.Empty;
            try
            {
                var nodePackageProvider = ServiceProvider.GetRequiredService<IPackedServiceCollection>();
                var type = nodePackageProvider.ShortName2Info[shortName].ServiceInstanceType;
                var obj = JsonConvert.DeserializeObject(jsonDecl, type);
                string? uid = type.BaseType?.GetProperty("TypeUID")?.GetValue(obj) as string;
                viewModel.Text = context.Format(Text?.GetLocalized() ?? string.Empty
                    , shortName, uid ?? throw new NullReferenceException());
            }
            catch
            {
                viewModel.Text = context.Format(ErrorText?.GetLocalized() ?? string.Empty
                    , shortName);
            }
            viewModel.Icon = Icon;
        }
    }
}
