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
    /// <summary>
    /// Base class for all services who observes nodes and do something according to data inside nodes.
    /// </summary>
    /// <typeparam name="TService"> The service itself. </typeparam>
    /// <typeparam name="TContext"> The context that preserves frequently used data for this service. </typeparam>
    /// <typeparam name="TSettings"> The singleton settings used by this service during the lifecycle of the application. </typeparam>
    /// <remarks> 
    /// TODO: add parameter <see cref="TSettings"/> for all subclass on recursive functions 
    /// (ensure consistency of <see cref="TSettings"/> for all recursion call because it may be replaced) 
    /// </remarks>
    public abstract class NodeService<TService, TContext, TSettings>
        where TService : NodeService<TService, TContext, TSettings>
        where TContext : NodeContext<TSettings>
        where TSettings : ServiceExtraSettings<TSettings>, new()
    {
        protected static Func<TService> _defaultServiceGetter = () => throw new NotImplementedException();
        private static readonly Dictionary<string, PriorityQueue<TService, PackageInfo>> _registered = new();

        protected static TSettings ServiceSettings => ServiceExtraSettings<TSettings>.Instance;

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

        protected static TContext GetContextOfNode(NodeData node, LocalServiceParam localParam)
            => GetContextOfNode(node, localParam, ServiceSettings);

        protected static TContext GetContextOfNode(NodeData node, LocalServiceParam localParam, TSettings serviceSettings)
        {
            var service = GetServiceOfTypeID(node.TypeUID);
            return service.BuildContextForNode(node, localParam, serviceSettings);
        }

        [JsonProperty]
        public string TypeUID { get; protected set; } = string.Empty;

        public TContext GetEmptyContext(LocalServiceParam localParam)
        {
            return GetEmptyContext(localParam, ServiceSettings);
        }

        /// <summary>
        /// When overridden in derived class, obtain an empty context object.
        /// </summary>
        /// <param name="localParam"> The <see cref="LocalServiceParam"/> inside the context. </param>
        /// <param name="serviceSettings"> The <see cref="TSettings"> need to pass to the context. </param>
        /// <returns> The context with the type <see cref="TContext"/>. </returns>
        /// <exception cref="NotImplementedException"> 
        /// Thrown when <see cref="Activator.CreateInstance"/> returns null. 
        /// </exception>
        /// <remarks>
        /// It should be overridden in each derived class, if not, it will use reflection to create instance,
        /// which will lead to bad performance.
        /// </remarks>
        public virtual TContext GetEmptyContext(LocalServiceParam localParam, TSettings serviceSettings)
        {
            return (TContext?)Activator.CreateInstance(typeof(TContext), new object[] { localParam, serviceSettings })
                ?? throw new NotImplementedException(
                    $"{typeof(TContext)} have no constructor with parameter of type {typeof(LocalServiceParam)} and {typeof(TSettings)}.");
        }

        protected TContext BuildContextForNode(NodeData node, LocalServiceParam localSettings)
            => BuildContextForNode(node, localSettings, ServiceSettings);

        protected TContext BuildContextForNode(NodeData node, LocalServiceParam localSettings, TSettings serviceSettings)
        {
            TContext context = GetEmptyContext(localSettings, serviceSettings);
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

    internal class DefaultNodeService : NodeService<DefaultNodeService, DefaultNodeContext, DefaultServiceExtraSettings> { }
}
