using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Core.Command;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized
{
    public class LocalVariable : PropertyViewServiceBase
    {
        [JsonProperty] public string CountPropertyName { get; set; } = string.Empty;
        [JsonProperty] public PropertyViewEditorType? CountEditor { get; set; }
        [JsonProperty] public string CountText { get; set; } = string.Empty;
        [JsonProperty] public Dictionary<string, string> LocalizedCountText { get; private set; } = [];

        [JsonProperty] public string NamePrefix { get; set; } = string.Empty;
        [JsonProperty] public string ValuePrefix { get; set; } = string.Empty;
        [JsonProperty] public PropertyViewEditorType? NameValueEditor { get; set; }

        protected override IReadOnlyList<PropertyTabViewModel> ResolvePropertyViewModelOfNode(NodeData nodeData, PropertyViewContext context)
        {
            List<PropertyItemViewModelBase> properties = [];
            string countStr = nodeData.GetProperty(CountPropertyName, "0");
            int count = 0;
            if (int.TryParse(countStr, out var c))
            {
                count = c;
            }
            properties.Add(new BasicPropertyItemViewModel()
            {
                Name = LocalizedCountText.GetValueOrDefault(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, CountText),
                Value = count.ToString(),
                Type = CountEditor
            });
            for (int i = 0; i < count; i++)
            {
                properties.Add(new VariableDefinitionPropertyItemViewModel(
                    nodeData.GetProperty($"{NamePrefix}_{i}"),
                    nodeData.GetProperty($"{ValuePrefix}_{i}"))
                {
                    Type = NameValueEditor 
                });
            }
            var tab = new PropertyTabViewModel()
            {
                Caption = PropertyViewServiceProvider.DefaultViewI18NCaption
            };
            properties.ForEach(tab.Properties.Add);
            return new List<PropertyTabViewModel>() { tab };
        }

        protected override CommandBase? ResolveCommandOfEditingNode(NodeData nodeData, 
            PropertyViewContext context, IReadOnlyList<PropertyTabViewModel> propertyList, 
            int tabIndex, int itemIndex, string edited)
        {
            if (tabIndex != 0) return null;
            if (itemIndex != 0)
            {
                var idx = itemIndex - 1;
                VariableDefinitionPropertyItemViewModel.VariableDefinition? variableDef = null;
                try
                {
                    variableDef = JsonConvert.DeserializeObject<VariableDefinitionPropertyItemViewModel.VariableDefinition>(edited);
                }
                catch (Exception) { }
                if (variableDef != null)
                {
                    var commands = new List<CommandBase>();
                    var editName = EditPropertyCommand.CreateEditCommandOnDemand(nodeData, $"{NamePrefix}_{idx}", variableDef.Name);
                    var editValue = EditPropertyCommand.CreateEditCommandOnDemand(nodeData, $"{ValuePrefix}_{idx}", variableDef.Value);
                    if (editName != null) commands.Add(editName);
                    if (editValue != null) commands.Add(editValue);
                    return new CompositeCommand(commands);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return EditPropertyCommand.CreateEditCommandOnDemand(nodeData, CountPropertyName, edited);
            }
        }
    }
}
