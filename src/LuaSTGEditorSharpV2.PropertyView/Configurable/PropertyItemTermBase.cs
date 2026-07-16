using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable;

public abstract class PropertyItemTermBase(IServiceProvider serviceProvider) : IPropertyItemTerm
{
    [JsonProperty] public PropertyViewEditorType? Editor { get; protected set; }

    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    public virtual PropertyItemViewModelBase GetViewModel(EditorNode nodeData, PropertyViewContext context)
        => GetViewModel([nodeData], context);

    public abstract PropertyItemViewModelBase GetViewModel(
        IReadOnlyList<EditorNode> nodeData,
        PropertyViewContext context);
}
