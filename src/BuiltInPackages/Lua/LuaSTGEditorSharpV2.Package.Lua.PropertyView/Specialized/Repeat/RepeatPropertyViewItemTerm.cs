using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.Repeat
{
    [Inject(ServiceLifetime.Transient)]
    public class RepeatPropertyViewItemTerm(IServiceProvider serviceProvider) 
        : IMultipleFieldPropertyItemTerm<RepeatVariableDefinition>
    {
        [JsonProperty] public NodePropertyCapture? NameRule { get; set; }
        [JsonProperty] public NodePropertyCapture? InitRule { get; set; }
        [JsonProperty] public NodePropertyCapture? IncrementRule { get; set; }
        [JsonProperty] public PropertyViewEditorType? NameValueEditor { get; set; }

        public IReadOnlyList<PropertyItemViewModelBase> GetViewModel(NodeData nodeData, PropertyViewContext context, int count)
        {
            var token = new NodePropertyAccessToken(serviceProvider, nodeData, context);
            List<PropertyItemViewModelBase> properties = [];
            for (int i = 0; i < count; i++)
            {
                object idx = i;
                var vm = serviceProvider.GetRequiredService<RepeatVariableDefinitionPropertyItemViewModelFactory>()
                    .Create(this, i, nodeData, context.LocalParam);
                vm.Type = NameValueEditor;
                vm.SetProxy(
                    NameRule?.CaptureByFormat(token, idx) ?? string.Empty,
                    InitRule?.CaptureByFormat(token, idx) ?? string.Empty,
                    IncrementRule?.CaptureByFormat(token, idx) ?? string.Empty);
                properties.Add(vm);
            }
            return properties;
        }
    }
}
