using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Exception;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Settings;

namespace LuaSTGEditorSharpV2.Core
{
    public abstract class ContextualNodeServiceProvider<TService, TContext, TSettings>(IServiceProvider serviceProvider)
        : NodeServiceProvider<TService>(serviceProvider), ISettingsProvider<TSettings>
        where TService : class
        where TContext : NodeContextWithSettings<TSettings>
        where TSettings : class, new()
    {
        protected TSettings ServiceSettings { get; set; } = new();

        public object Settings
        {
            get => ServiceSettings ?? new();
            set
            {
                ServiceSettings = (value as TSettings) ?? ServiceSettings;
            }
        }

        public virtual void RefreshSettings() { }

        public void LoadSettings(TSettings settings)
        {
            ServiceSettings = settings;
        }

        internal protected TContext GetContextOfNode(NodeData node, LocalServiceParam localParam)
            => GetContextOfNode(node, localParam, ServiceSettings);

        internal protected TContext GetContextOfNode(NodeData node, LocalServiceParam localParam, TSettings serviceSettings)
        {
            return BuildContextForNode(node, localParam, serviceSettings);
        }

        public IEnumerable<NodeServicePair<UService>> GetServicesPairForLogicalChildrenOfType<UService>(NodeData nodeData)
            where UService : TService
        {
            foreach (var n in nodeData.GetLogicalChildren())
            {
                var s = GetServiceOfNode(n);
                if (s is UService service)
                {
                    yield return new NodeServicePair<UService>(service, n);
                }
            }
        }

        protected TService GetServiceOfTypeID(string typeUID)
            => GetServiceInstanceOfTypeUID(typeUID);

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

    internal class DefaultNodeServiceProvider : ContextualNodeServiceProvider<DefaultNodeService, DefaultNodeContext, ServiceExtraSettingsBase>
    {
        private readonly DefaultNodeService _default;

        protected override DefaultNodeService DefaultService => _default;

        public DefaultNodeServiceProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _default = new(this, serviceProvider);
        }
    }
}
