using System;
using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.LocalVariable;

[Inject(ServiceLifetime.Transient)]
public class LocalVariablePropertyViewItemListTerm(IServiceProvider serviceProvider)
    : PropertyItemListTermBase(serviceProvider)
{
    [JsonProperty(Required = Required.Always)] public NodePropertyCapture NameRule { get; set; } = null!;
    [JsonProperty(Required = Required.Always)] public NodePropertyCapture ValueRule { get; set; } = null!;
    [JsonProperty] public PropertyViewEditorType? NameValueEditor { get; set; }

    public override IReadOnlyList<PropertyItemViewModelBase> GetViewModels(
        IReadOnlyList<EditorNode> nodes,
        PropertyViewContext context,
        int count)
    {
        List<PropertyItemViewModelBase> properties = [];
        for (var i = 0; i < count; i++)
        {
            var itemTerm = new ItemTerm(ServiceProvider, NameValueEditor)
            {
                NameRule = NameRule.Format(i),
                ValueRule = ValueRule.Format(i),
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

        public NodePropertyCapture NameRule { get; init; } = null!;
        public NodePropertyCapture ValueRule { get; init; } = null!;

        public override PropertyItemViewModelBase GetViewModel(IReadOnlyList<EditorNode> nodes, PropertyViewContext context)
        {
            var factory = ServiceProvider.GetRequiredService<
                IPropertyItemViewModelFactory<VariableDefinitionPropertyItemViewModel, ItemTerm>>();
            return factory.Create(nodes, this, Editor, context);
        }
    }
}
