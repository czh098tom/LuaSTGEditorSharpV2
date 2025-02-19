using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.NodeProfile
{
    [Inject(ServiceLifetime.Transient)]
    public class NodeProfileGenerator(IServiceProvider serviceProvider)
    {
        public IEnumerable<NodeProfile> CreateProfile(NodeProfileFilter filter = NodeProfileFilter.ActiveOnly)
        {
            Dictionary<string, List<ServiceProfile>> result = [];
            var collection = serviceProvider.GetRequiredService<IPackedServiceCollection>();
            foreach (var item in collection)
            {
                var providerType = item.ServiceProviderType;
                if (providerType
                    .BaseTypes()
                    .Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(NodeServiceProvider<>)))
                {
                    if (serviceProvider.GetRequiredService(providerType) is IPackedServiceInstanceCollection packedServiceInstanceCollection)
                    {
                        if (filter == NodeProfileFilter.None)
                        {
                            var dict = packedServiceInstanceCollection.GetAllRegistered();
                            foreach (var kvp in dict)
                            {
                                var list = result.GetOrAdd(kvp.Key);
                                foreach (var (data, packageInfo) in kvp.Value)
                                {
                                    list.Add(new ServiceProfile(item.Name, data, packageInfo));
                                }
                            }
                        }
                        else if (filter == NodeProfileFilter.ActiveOnly)
                        {
                            var dict = packedServiceInstanceCollection.GetRegisteredAvailableData();
                            foreach (var kvp in dict)
                            {
                                var list = result.GetOrAdd(kvp.Key);
                                list.Add(new ServiceProfile(item.Name, kvp.Value.data, kvp.Value.packageInfo));
                            }
                        }
                    }
                }
            }
            return result.Select(kvp => new NodeProfile(kvp.Key, [.. kvp.Value]));
        }
    }
}
