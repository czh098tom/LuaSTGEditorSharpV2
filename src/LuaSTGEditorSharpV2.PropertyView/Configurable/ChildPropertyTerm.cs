using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    [Inject(ServiceLifetime.Transient)]
    [JsonTypeShortName(typeof(IPropertyItemTerm), "ChildProperty")]
    public class ChildPropertyTerm(IServiceProvider serviceProvider,
        PropertyViewServiceProvider propertyViewServiceProvider,
        EditorNodeFactory editorNodeFactory) : IPropertyItemTerm
    {
        [JsonProperty] public NodePropertyCapture? FindProperty { get; private set; }
        [JsonProperty] public HashSet<string?>? OfName { get; private set; }

        public PropertyItemViewModelBase GetViewModel(EditorNode nodeData, PropertyViewContext context)
        {
            return GetViewModelImpl(nodeData, context);
        }

        public PropertyTabWrapperItemViewModel GetViewModelImpl(EditorNode nodeData, PropertyViewContext context)
        {
            var service = propertyViewServiceProvider;

            using var _ = context.AcquireContextLevelHandle(nodeData.Source);
            var pairs = service.GetServicesPairForLogicalChildrenOfType<PropertyViewServiceBase>(
                nodeData.Source)
                .Where(p =>
                {
                    var token = new NodePropertyAccessToken(serviceProvider, p.NodeData, context);
                    return OfName?.Contains(FindProperty?.Capture(token)) ?? true;
                });

            var viewModel = new PropertyTabWrapperItemViewModel
            {
                Type = new PropertyViewEditorType("childNode")
            };
            viewModel.Initialize(
                [.. pairs.Select(p => service.GetPropertyViewModelOfNode(
                    editorNodeFactory.GetOrCreate(p.NodeData, nodeData.Document), context)[0])],
                nodeData,
                context.LocalParam,
                serviceProvider.GetRequiredService<PropertyEditWizardProviderService>());
            return viewModel;
        }
    }
}
