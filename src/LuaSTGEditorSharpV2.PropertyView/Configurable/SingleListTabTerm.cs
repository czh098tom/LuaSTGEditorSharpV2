using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    [Inject(ServiceLifetime.Transient)]
    public class SingleListTabTerm<TTermVariable>(
        IServiceProvider serviceProvider,
        PropertyViewServiceProvider propertyViewProvider,
        DefaultValueServiceProvider defaultValueServiceProvider)
        : PropertyTabTermBase(serviceProvider, propertyViewProvider), IMultiSourcePropertyTabTerm
        where TTermVariable : class, IPropertyItemListTerm
    {
        [JsonProperty] public IPropertyItemTerm[] ImmutableProperty { get; private set; } = [];
        [JsonProperty] public PropertyItemTerm? Count { get; private set; } = null;
        [JsonProperty] public TTermVariable? VariableProperty { get; private set; } = null;

        public override PropertyTabViewModel GetPropertyTabViewModel(EditorNode nodeData, PropertyViewContext context)
        {
            List<PropertyItemViewModelBase> properties = [];
            for (int i = 0; i < ImmutableProperty.Length; i++)
            {
                properties.Add(ImmutableProperty[i].GetViewModel(nodeData, context));
            }
            if (Count != null && VariableProperty != null)
            {
                var count = GetCount(nodeData, context);
                properties.Add(Count.GetViewModel(nodeData, context));
                properties.AddRange(VariableProperty.GetViewModels(nodeData, context, count));
            }
            var tab = new PropertyTabViewModel()
            {
                Caption = Caption?.GetLocalized() ?? PropertyViewServiceProvider.DefaultViewI18NCaption,
            };
            properties.ForEach(tab.Properties.Add);
            return tab;
        }

        public PropertyTabViewModel GetPropertyTabViewModel(
            IReadOnlyList<EditorNode> nodeData,
            PropertyViewContext context)
        {
            List<PropertyItemViewModelBase> properties = [];
            foreach (var term in ImmutableProperty)
            {
                if (term is IMultiSourcePropertyItemTerm multiSourceTerm)
                {
                    properties.Add(multiSourceTerm.GetViewModel(nodeData, context));
                }
                else
                {
                    var placeholder = new UnsupportedMultiSourcePropertyItemViewModel(
                        PropertyViewServiceProvider.MultiSelectionUnsupportedI18NText);
                    placeholder.Initialize(
                        nodeData,
                        context.LocalParam,
                        ServiceProvider.GetRequiredService<PropertyEditWizardProviderService>());
                    properties.Add(placeholder);
                }
            }

            if (Count != null && VariableProperty != null)
            {
                var counts = nodeData.Select(node => GetCount(node, context)).ToArray();
                properties.Add(Count.GetViewModel(nodeData, context));
                if (counts.Length > 0 && counts.All(count => count == counts[0]))
                {
                    properties.AddRange(VariableProperty.GetViewModels(
                        nodeData,
                        context,
                        counts[0]));
                }
            }

            var tab = new PropertyTabViewModel(true)
            {
                Caption = Caption?.GetLocalized() ?? PropertyViewServiceProvider.DefaultViewI18NCaption,
            };
            properties.ForEach(tab.Properties.Add);
            return tab;
        }

        private int GetCount(EditorNode nodeData, PropertyViewContext context)
        {
            var token = defaultValueServiceProvider.GetToken(nodeData.Source, context);
            var countValue = Count?.Mapping?.Capture(token) ?? string.Empty;
            return int.TryParse(countValue, out var count) ? count : 0;
        }
    }
}
