using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.LocalVariable
{
    public class LocalVariablePropertyViewItemTerm : IMultipleFieldPropertyViewItemTerm<VariableDefinition>
    {
        [JsonProperty] public NodePropertyCapture? NameRule { get; set; }
        [JsonProperty] public NodePropertyCapture? ValueRule { get; set; }
        [JsonProperty] public PropertyViewEditorType? NameValueEditor { get; set; }

        public IReadOnlyList<PropertyItemViewModelBase> GetViewModel(NodeData nodeData, int count)
        {
            List<PropertyItemViewModelBase> properties = [];
            for (int i = 0; i < count; i++)
            {
                object idx = i;
                properties.Add(new VariableDefinitionPropertyItemViewModel(
                    NameRule?.CaptureByFormat(nodeData, idx) ?? string.Empty,
                    ValueRule?.CaptureByFormat(nodeData, idx) ?? string.Empty)
                {
                    Type = NameValueEditor
                });
            }
            return properties;
        }

        public CommandBase? GetCommand(NodeData nodeData, VariableDefinition intermediateModel, int index)
        {
            var commands = new List<CommandBase>();
            if (NameRule == null || ValueRule == null) return null;
            object idx = index;
            var editName = EditPropertyCommand.CreateEditCommandOnDemand(nodeData, string.Format(NameRule.Key, idx), intermediateModel.Name);
            var editValue = EditPropertyCommand.CreateEditCommandOnDemand(nodeData, string.Format(ValueRule.Key, idx), intermediateModel.Value);
            if (editName != null) commands.Add(editName);
            if (editValue != null) commands.Add(editValue);
            return new CompositeCommand(commands);
        }
    }
}
