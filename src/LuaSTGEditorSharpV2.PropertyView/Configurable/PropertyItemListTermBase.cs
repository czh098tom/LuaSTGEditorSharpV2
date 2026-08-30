using System;
using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable;

public abstract class PropertyItemListTermBase(IServiceProvider serviceProvider)
    : IMultiSourcePropertyItemListTerm
{
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    public IReadOnlyList<PropertyItemViewModelBase> GetViewModels(
        EditorNode nodeData,
        PropertyViewContext context,
        int count)
        => GetViewModels([nodeData], context, count);

    public abstract IReadOnlyList<PropertyItemViewModelBase> GetViewModels(
        IReadOnlyList<EditorNode> nodes,
        PropertyViewContext context,
        int count);
}
