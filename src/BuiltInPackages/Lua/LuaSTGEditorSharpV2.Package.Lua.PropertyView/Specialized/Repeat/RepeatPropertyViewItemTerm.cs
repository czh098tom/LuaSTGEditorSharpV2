using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.PropertyView.Configurable;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.Repeat
{
    public class RepeatPropertyViewItemTerm : IMultipleFieldPropertyViewItemTerm<RepeatVariableDefinition>
    {
        [JsonProperty] public NodePropertyCapture? NameRule { get; set; }
        [JsonProperty] public NodePropertyCapture? InitRule { get; set; }
        [JsonProperty] public NodePropertyCapture? IncrementRule { get; set; }
        [JsonProperty] public PropertyViewEditorType? NameValueEditor { get; set; }

        public IReadOnlyList<PropertyItemViewModelBase> GetViewModel(NodeData nodeData, PropertyViewContext context, int count)
        {
            var token = new NodePropertyAccessToken(nodeData, context);
            List<PropertyItemViewModelBase> properties = [];
            for (int i = 0; i < count; i++)
            {
                object idx = i;
                properties.Add(new RepeatVariableDefinitionPropertyItemViewModel(
                    NameRule?.CaptureByFormat(token, idx) ?? string.Empty,
                    InitRule?.CaptureByFormat(token, idx) ?? string.Empty,
                    IncrementRule?.CaptureByFormat(token, idx) ?? string.Empty,
                    nodeData, context.LocalParam)
                {
                    Type = NameValueEditor
                });
            }
            return properties;
        }

        public CommandBase? GetCommand(NodeData nodeData, RepeatVariableDefinition intermediateModel, int index)
        {
            var commands = new List<CommandBase>();
            if (NameRule == null || InitRule == null || IncrementRule == null) return null;
            object idx = index;
            var editName = EditPropertyCommand.CreateEditCommandOnDemand(nodeData, string.Format(NameRule.Key, idx), intermediateModel.Name);
            var editValue = EditPropertyCommand.CreateEditCommandOnDemand(nodeData, string.Format(InitRule.Key, idx), intermediateModel.Init);
            var editIncrement = EditPropertyCommand.CreateEditCommandOnDemand(nodeData, string.Format(IncrementRule.Key, idx), intermediateModel.Increment);
            if (editName != null) commands.Add(editName);
            if (editValue != null) commands.Add(editValue);
            if (editIncrement != null) commands.Add(editIncrement);
            return commands.Count > 0 ? new CompositeCommand(commands) : null;
        }
    }
}
