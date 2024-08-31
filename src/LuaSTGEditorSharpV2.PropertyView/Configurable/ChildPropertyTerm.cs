using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    [Inject(ServiceLifetime.Transient)]
    public class ChildPropertyTerm(IServiceProvider serviceProvider, PropertyViewServiceProvider propertyViewServiceProvider) : IPropertyViewTerm
    {
        [JsonProperty] public NodePropertyCapture? FindProperty { get; private set; }
        [JsonProperty] public HashSet<string?>? OfName { get; private set; }

        public PropertyItemViewModelBase GetViewModel(NodeData nodeData, PropertyViewContext context)
        {
            return GetViewModelImpl(nodeData, context);
        }

        public PropertyTabWrapperItemViewModel GetViewModelImpl(NodeData nodeData, PropertyViewContext context)
        {
            var service = propertyViewServiceProvider;

            using var _ = context.AcquireContextLevelHandle(nodeData);
            var pairs = service.GetServicesPairForLogicalChildrenOfType<PropertyViewServiceBase>(
                nodeData)
                .Where(p =>
                {
                    var token = new NodePropertyAccessToken(serviceProvider, p.NodeData, context);
                    return OfName?.Contains(FindProperty?.Capture(token)) ?? true;
                });

            return new PropertyTabWrapperItemViewModel(
                pairs.Select(p => service.GetPropertyViewModelOfNode(p.NodeData, context)[0])
                .ToList(),
                nodeData,
                context.LocalParam)
            {
                Type = new PropertyViewEditorType("childNode")
            };
        }

        public CommandBase? ResolveCommandOfEditingNode(NodeData nodeData, PropertyViewContext context, string edited)
        {
            return null;
        }
    }
}
