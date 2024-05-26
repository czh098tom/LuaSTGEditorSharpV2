using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    public class PropertyViewTerm(
        NodePropertyCapture? mapping,
        LocalizableString caption,
        PropertyViewEditorType? editor = null,
        bool enabled = true) : IPropertyViewTerm
    {
        [JsonProperty] public NodePropertyCapture? Mapping { get; private set; } = mapping;
        [JsonProperty] public LocalizableString Caption { get; private set; } = caption;
        [JsonProperty] public PropertyViewEditorType? Editor { get; private set; } = editor;
        [JsonProperty] public bool Enabled { get; private set; } = enabled;

        public PropertyViewTerm() : this(null, new(), null, true) { }

        public PropertyItemViewModelBase GetViewModel(NodeData nodeData, PropertyViewContext context)
        {
            var token = new NodePropertyAccessToken(nodeData, context);
            return new BasicPropertyItemViewModel(nodeData, context.LocalParam)
            {
                Name = Caption.GetLocalized(),
                Value = Mapping?.Capture(token) ?? string.Empty,
                Type = Editor,
                Enabled = Enabled
            };
        }

        public CommandBase? ResolveCommandOfEditingNode(NodeData nodeData, PropertyViewContext context, string edited)
        {
            return EditPropertyCommand.CreateEditCommandOnDemand(nodeData, Mapping?.Key, edited);
        }
    }
}
