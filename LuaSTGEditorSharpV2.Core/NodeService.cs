using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private static readonly Dictionary<string, TService> _registered = new();

        protected static void Register(string typeID, TService service)
        {
            try
            {
                _registered.Add(typeID, service);
            }
            catch (ArgumentException e)
            {
                throw new DuplicatedTypeIDException(typeID, e);
            }
        }

        protected static TService GetServiceOfTypeID(string typeUID)
        {
            if (_registered.ContainsKey(typeUID))
            {
                return _registered[typeUID];
            }
            else
            {
                return _defaultServiceGetter();
            }
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
                ?? throw new NotImplementedException($"{typeof(TContext)} have no constructor with parameter of type {typeof(LocalSettings)}.");
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
