using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Exception;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core
{
    public abstract class NodeServiceProvider<TServiceProvider, TService, TContext, TSettings>
        where TServiceProvider : NodeServiceProvider<TServiceProvider, TService, TContext, TSettings>
        where TService : NodeService<TServiceProvider, TService, TContext, TSettings>
        where TContext : NodeContext<TSettings>
        where TSettings : ServiceExtraSettings<TSettings>, new ()
    {
        private readonly Dictionary<string, PriorityQueue<TService, PackageInfo>> _registered = [];

        protected static TSettings ServiceSettings => ServiceExtraSettings<TSettings>.Instance;

        public void Register(string typeID, PackageInfo packageInfo, TService service)
        {
            try
            {
                if (!_registered.ContainsKey(typeID))
                {
                    _registered.Add(typeID, new PriorityQueue<TService, PackageInfo>());
                }
                _registered[typeID].Enqueue(service, packageInfo);
            }
            catch (ArgumentException e)
            {
                throw new DuplicatedTypeIDException(typeID, e);
            }
        }

        public void UnloadPackage(PackageInfo packageInfo)
        {
            List<TService> services = [];
            List<PackageInfo> packageInfos = [];
            foreach (var kvp in _registered)
            {
                var pq = kvp.Value;
                services.Clear();
                packageInfos.Clear();
                services.Capacity = services.Capacity < pq.Count ? pq.Count : services.Capacity;
                packageInfos.Capacity = packageInfos.Capacity < pq.Count ? pq.Count : packageInfos.Capacity;
                while (pq.TryDequeue(out TService? s, out PackageInfo? pi))
                {
                    if (pi != packageInfo)
                    {
                        services.Add(s);
                        packageInfos.Add(pi);
                    }
                }
                for (int i = 0; i < services.Count; i++)
                {
                    pq.Enqueue(services[i], packageInfos[i]);
                }
            }
        }

        protected abstract TService DefaultService { get; }

        internal protected TService GetServiceOfTypeID(string typeUID)
        {
            if (_registered.ContainsKey(typeUID) && _registered[typeUID].Count > 0)
            {
                return _registered[typeUID].Peek();
            }
            return DefaultService;
        }

        internal protected TService GetServiceOfNode(NodeData node)
        {
            return GetServiceOfTypeID(node.TypeUID);
        }

        internal protected TContext GetContextOfNode(NodeData node, LocalServiceParam localParam)
            => GetContextOfNode(node, localParam, ServiceSettings);

        internal protected TContext GetContextOfNode(NodeData node, LocalServiceParam localParam, TSettings serviceSettings)
        {
            var service = GetServiceOfTypeID(node.TypeUID);
            return service.BuildContextForNode(node, localParam, serviceSettings);
        }
    }

    internal class DefaultNodeServiceProvider : NodeServiceProvider<DefaultNodeServiceProvider, DefaultNodeService, DefaultNodeContext, DefaultServiceExtraSettings> 
    {
        private static readonly DefaultNodeService _default = new();

        protected override DefaultNodeService DefaultService => _default;
    }
}
