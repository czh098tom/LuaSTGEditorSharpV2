using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Core.Command;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.LocalVariable
{
    public class LocalVariablePropertyViewItemTerm : IMultipleFieldPropertyViewItemTerm<VariableDefinition>
    {
        [JsonProperty] public string NamePrefix { get; set; } = string.Empty;
        [JsonProperty] public string ValuePrefix { get; set; } = string.Empty;
        [JsonProperty] public PropertyViewEditorType? NameValueEditor { get; set; }

        public IReadOnlyList<PropertyItemViewModelBase> GetViewModel(NodeData nodeData, int count)
        {
            List<PropertyItemViewModelBase> properties = [];
            for (int i = 0; i < count; i++)
            {
                properties.Add(new VariableDefinitionPropertyItemViewModel(
                    nodeData.GetProperty($"{NamePrefix}_{i}"),
                    nodeData.GetProperty($"{ValuePrefix}_{i}"))
                {
                    Type = NameValueEditor
                });
            }
            return properties;
        }

        public CommandBase? GetCommand(NodeData nodeData, VariableDefinition intermediateModel, int index)
        {
            var commands = new List<CommandBase>();
            var editName = EditPropertyCommand.CreateEditCommandOnDemand(nodeData, $"{NamePrefix}_{index}", intermediateModel.Name);
            var editValue = EditPropertyCommand.CreateEditCommandOnDemand(nodeData, $"{ValuePrefix}_{index}", intermediateModel.Value);
            if (editName != null) commands.Add(editName);
            if (editValue != null) commands.Add(editValue);
            return new CompositeCommand(commands);
        }
    }
}
