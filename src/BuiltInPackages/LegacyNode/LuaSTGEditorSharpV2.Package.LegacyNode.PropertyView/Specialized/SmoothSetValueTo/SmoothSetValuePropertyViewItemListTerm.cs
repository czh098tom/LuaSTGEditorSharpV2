using System;
using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.PropertyView.Specialized.SmoothSetValueTo;

[Inject(ServiceLifetime.Transient)]
public class SmoothSetValuePropertyViewItemListTerm(IServiceProvider serviceProvider)
    : PropertyItemListTermBase(serviceProvider)
{
    [JsonProperty(Required = Required.Always)] public NodePropertyCapture VariableNameRule { get; set; } = null!;
    [JsonProperty(Required = Required.Always)] public NodePropertyCapture TargetValueRule { get; set; } = null!;
    [JsonProperty(Required = Required.Always)] public NodePropertyCapture InterpolationModeRule { get; set; } = null!;
    [JsonProperty(Required = Required.Always)] public NodePropertyCapture ModificationModeRule { get; set; } = null!;
    [JsonProperty] public PropertyViewEditorType? Editor { get; set; }

    public override IReadOnlyList<PropertyItemViewModelBase> GetViewModels(
        IReadOnlyList<EditorNode> nodes,
        PropertyViewContext context,
        int count)
    {
        List<PropertyItemViewModelBase> properties = [];
        for (var i = 0; i < count; i++)
        {
            var itemTerm = new ItemTerm(ServiceProvider, Editor)
            {
                VariableName = VariableNameRule.Format(i),
                TargetValue = TargetValueRule.Format(i),
                InterpolationMode = InterpolationModeRule.Format(i),
                ModificationMode = ModificationModeRule.Format(i),
            };
            properties.Add(itemTerm.GetViewModel(nodes, context));
        }
        return properties;
    }

    public sealed class ItemTerm : PropertyItemTermBase
    {
        public ItemTerm(IServiceProvider serviceProvider, PropertyViewEditorType? editor)
            : base(serviceProvider)
        {
            Editor = editor;
        }

        public NodePropertyCapture VariableName { get; init; } = null!;
        public NodePropertyCapture TargetValue { get; init; } = null!;
        public NodePropertyCapture InterpolationMode { get; init; } = null!;
        public NodePropertyCapture ModificationMode { get; init; } = null!;

        public override PropertyItemViewModelBase GetViewModel(
            IReadOnlyList<EditorNode> nodes,
            PropertyViewContext context)
        {
            var factory = ServiceProvider.GetRequiredService<
                IPropertyItemViewModelFactory<SmoothSetValueDefinitionPropertyItemViewModel, ItemTerm>>();
            return factory.Create(nodes, this, Editor, context);
        }
    }
}
