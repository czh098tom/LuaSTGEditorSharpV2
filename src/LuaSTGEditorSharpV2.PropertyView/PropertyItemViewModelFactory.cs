using System;
using System.Collections.Generic;
using System.Linq;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.PropertyView;

[Inject(ServiceLifetime.Singleton, typeof(IPropertyItemViewModelFactory<,>))]
public class PropertyItemViewModelFactory<TViewModel, TTerm>(
    DefaultValueServiceProvider defaultValueServiceProvider,
    PropertyEditWizardProviderService propertyEditWizardProviderService)
    : IPropertyItemViewModelFactory<TViewModel, TTerm>
    where TTerm : PropertyItemTermBase
    where TViewModel : BoundPropertyItemViewModelBase<TTerm>, new()
{
    public TViewModel Create(
        IReadOnlyList<EditorNode> nodes,
        TTerm term,
        PropertyViewEditorType? type,
        PropertyViewContext context)
    {
        var sources = nodes.Select(node => new PropertySource(
            node, defaultValueServiceProvider.GetToken(node.Source, context))).ToArray();
        var viewModel = new TViewModel();
        viewModel.Initialize(sources, context.LocalParam, propertyEditWizardProviderService);
        viewModel.Type = type;
        viewModel.Configure(term);
        viewModel.Populate();
        return viewModel;
    }
}
