using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.LocalVariable
{
    [Inject(ServiceLifetime.Transient)]
    public class LocalVariablePropertyViewItemTerm(IServiceProvider serviceProvider, ViewModelProviderServiceProvider viewModelProviderServiceProvider)
        : IMultipleFieldPropertyViewItemTerm<VariableDefinition>
    {
        [JsonProperty] public NodePropertyCapture? NameRule { get; set; }
        [JsonProperty] public NodePropertyCapture? ValueRule { get; set; }
        [JsonProperty] public PropertyViewEditorType? NameValueEditor { get; set; }

        public IReadOnlyList<PropertyItemViewModelBase> GetViewModel(NodeData nodeData, PropertyViewContext context, int count)
        {
            var token = new NodePropertyAccessToken(serviceProvider, nodeData, context);
            List<PropertyItemViewModelBase> properties = [];
            for (int i = 0; i < count; i++)
            {
                object idx = i;
                var vm = serviceProvider.GetRequiredService<VariableDefinitionPropertyItemViewModelFactory>()
                    .Create(this, i, nodeData, context.LocalParam);
                vm.Type = NameValueEditor;
                vm.SetProxy(NameRule?.CaptureByFormat(token, idx) ?? string.Empty,
                    ValueRule?.CaptureByFormat(token, idx) ?? string.Empty);
                properties.Add(vm);
            }
            return properties;
        }

        public CommandBase? GetCommand(NodeData nodeData, VariableDefinition intermediateModel, int index)
        {
            var commands = new List<CommandBase>();
            if (NameRule == null || ValueRule == null) return null;
            object idx = index;
            var editName = EditPropertyCommand.CreateEditCommandOnDemand(viewModelProviderServiceProvider, nodeData, string.Format(NameRule.Key, idx), intermediateModel.Name);
            var editValue = EditPropertyCommand.CreateEditCommandOnDemand(viewModelProviderServiceProvider, nodeData, string.Format(ValueRule.Key, idx), intermediateModel.Value);
            if (editName != null) commands.Add(editName);
            if (editValue != null) commands.Add(editValue);
            return commands.Count > 0 ? new CompositeCommand(commands) : null;
        }
    }
}
