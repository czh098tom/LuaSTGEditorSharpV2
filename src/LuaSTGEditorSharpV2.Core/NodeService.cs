using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Exception;

namespace LuaSTGEditorSharpV2.Core
{
    public abstract class NodeService<TService, TContext>
        where TService : NodeService<TService, TContext>
        where TContext : NodeContext
    {
        protected static Func<TService> _defaultServiceGetter = () => throw new NotImplementedException();
        private static readonly Dictionary<string, PriorityQueue<TService, PackageInfo>> _registered = new();

        public static void Register(string typeID, PackageInfo packageInfo, TService service)
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

        public static void UnloadPackage(PackageInfo packageInfo)
        {
            List<TService> services = new();
            List<PackageInfo> packageInfos = new();
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

        protected static TService GetServiceOfTypeID(string typeUID)
        {
            if (_registered.ContainsKey(typeUID) && _registered[typeUID].Count > 0)
            {
                return _registered[typeUID].Peek();
            }
            return _defaultServiceGetter();
        }

        protected static TService GetServiceOfNode(NodeData node)
        {
            return GetServiceOfTypeID(node.TypeUID);
        }

        protected static TContext GetContextOfNode(NodeData node, LocalSettings localSettings)
        {
            var service = GetServiceOfTypeID(node.TypeUID);
            return service.BuildContextForNode(node, localSettings);
        }

        [JsonProperty]
        public string TypeUID { get; protected set; } = string.Empty;

        public virtual TContext GetEmptyContext(LocalSettings localSettings)
        {
            return (TContext?)Activator.CreateInstance(typeof(TContext), new object[] { localSettings })
                ?? throw new NotImplementedException(
                    $"{typeof(TContext)} have no constructor with parameter of type {typeof(LocalSettings)}.");
        }

        protected TContext BuildContextForNode(NodeData node, LocalSettings localSettings)
        {
            TContext context = GetEmptyContext(localSettings);
            Stack<NodeData> stack = new();
            NodeData? curr = node.PhysicalParent;
            while (curr != null)
            {
                stack.Push(curr);
                curr = curr.PhysicalParent;
            }
            while (stack.Count > 0)
            {
                context.Push(stack.Pop());
            }
            return context;
        }
    }
}
