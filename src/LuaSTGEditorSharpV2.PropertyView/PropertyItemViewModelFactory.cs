using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.PropertyView;

[Inject(ServiceLifetime.Singleton, typeof(IPropertyItemViewModelFactory<,>))]
public class PropertyItemViewModelFactory<TViewModel, TTerm>(
    PropertyEditWizardProviderService propertyEditWizardProviderService)
    : IPropertyItemViewModelFactory<TViewModel, TTerm>
    where TTerm: PropertyItemTermBase
    where TViewModel: BoundPropertyItemViewModelBase<TTerm>, new()
{
    public TViewModel Create(IReadOnlyList<PropertySource> nodeData, TTerm term,
        PropertyViewEditorType? type, LocalServiceParam localServiceParam)
    {
        var vm = new TViewModel();
        vm.Initialize(nodeData, localServiceParam, propertyEditWizardProviderService);
        vm.Type = type;
        vm.Configure(term);
        vm.Populate();
        return vm;
    }
}
