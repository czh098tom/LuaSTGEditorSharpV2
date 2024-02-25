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
    public abstract class CompactNodeServiceProvider<TServiceProvider, TService, TContext, TSettings> 
        : NodeServiceProvider<TService>, ISettingsProvider<TSettings>
        where TServiceProvider : CompactNodeServiceProvider<TServiceProvider, TService, TContext, TSettings>
        where TService : CompactNodeService<TServiceProvider, TService, TContext, TSettings>
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
            var service = GetServiceInstanceOfTypeUID(node.TypeUID);
            return service.BuildContextForNode(node, localParam, serviceSettings);
        }
    }

    internal class DefaultNodeServiceProvider : CompactNodeServiceProvider<DefaultNodeServiceProvider, DefaultNodeService, DefaultNodeContext, ServiceExtraSettingsBase> 
    {
        private static readonly DefaultNodeService _default = new();

        protected override DefaultNodeService DefaultService => _default;
    }
}
