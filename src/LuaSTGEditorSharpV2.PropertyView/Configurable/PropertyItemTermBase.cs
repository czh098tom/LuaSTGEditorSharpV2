using System;
using Microsoft.Extensions.DependencyInjection;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable;

public abstract class PropertyItemTermBase(IServiceProvider serviceProvider) : IPropertyItemTerm
{
    public abstract PropertyItemViewModelBase GetViewModel(EditorNode nodeData, PropertyViewContext context);

    protected TResult GetViewModelImpl<TResult, TTerm>(EditorNode nodeData, PropertyViewContext context,
        TTerm term, PropertyViewEditorType? type)
        where TResult : BoundPropertyItemViewModelBase<TTerm>
        where TTerm : PropertyItemTermBase
    {
        var token = new NodePropertyAccessToken(serviceProvider, nodeData.Source, context);
        var factory = serviceProvider.GetRequiredService<IPropertyItemViewModelFactory<TResult, TTerm>>();
        var viewModel = factory.Create([new PropertySource(nodeData, token)], term, type, context.LocalParam);
        return viewModel;
    }
}
