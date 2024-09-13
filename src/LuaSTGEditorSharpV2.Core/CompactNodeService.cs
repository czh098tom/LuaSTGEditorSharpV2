﻿using System;
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
    public abstract class CompactNodeService<TNodeServiceProvider, TService, TContext, TSettings>(TNodeServiceProvider nodeServiceProvider, IServiceProvider serviceProvider) : NodeServiceBase(serviceProvider)
        where TNodeServiceProvider : CompactNodeServiceProvider<TNodeServiceProvider, TService, TContext, TSettings>
        where TService : CompactNodeService<TNodeServiceProvider, TService, TContext, TSettings>
        where TContext : NodeContextWithSettings<TSettings>
        where TSettings : class, new()
    {
        [JsonIgnore]
        protected TNodeServiceProvider NodeServiceProvider { get; private set; } = nodeServiceProvider;

        protected TNodeServiceProvider GetNodeServiceProvider()
        {
            return NodeServiceProvider;
        }

        protected TContext GetContextOfNode(NodeData node, LocalServiceParam localParam, TSettings serviceSettings)
        {
            var service = GetNodeServiceProvider().GetServiceInstanceOfTypeUID(node.TypeUID);
            return service.BuildContextForNode(node, localParam, serviceSettings);
        }

        protected TService GetServiceOfTypeID(string typeUID)
            => GetNodeServiceProvider().GetServiceInstanceOfTypeUID(typeUID);

        protected TService GetServiceOfNode(NodeData node)
            => GetNodeServiceProvider().GetServiceOfNode(node);

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
            return (TContext?)Activator.CreateInstance(typeof(TContext), [localParam, serviceSettings])
                ?? throw new NotImplementedException(
                    $"{typeof(TContext)} have no constructor with parameter of type {typeof(LocalServiceParam)} and {typeof(TSettings)}.");
        }

        internal TContext BuildContextForNode(NodeData node, LocalServiceParam localSettings, TSettings serviceSettings)
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
                context.AcquireContextLevelHandle(stack.Pop());
            }
            return context;
        }
    }

    internal class DefaultNodeService(DefaultNodeServiceProvider nodeServiceProvider, IServiceProvider serviceProvider) 
        : CompactNodeService<DefaultNodeServiceProvider, DefaultNodeService, DefaultNodeContext, ServiceExtraSettingsBase>(nodeServiceProvider, serviceProvider)
    {
    }
}
