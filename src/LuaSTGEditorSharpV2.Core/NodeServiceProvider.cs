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
    public abstract class NodeServiceProvider<TServiceProvider, TService, TContext, TSettings> 
        : PackedDataProviderServiceBase<TService>, INodeServiceProvider, ISettingsProvider<TSettings>
        where TServiceProvider : NodeServiceProvider<TServiceProvider, TService, TContext, TSettings>
        where TService : NodeService<TServiceProvider, TService, TContext, TSettings>
        where TContext : NodeContext<TSettings>
        where TSettings : class, new()
    {
        protected abstract TService DefaultService { get; }

        protected TSettings ServiceSettings { get; set; } = new();

        public object Settings
        {
            get => ServiceSettings ?? new();
            set
            {
                ServiceSettings = (value as TSettings) ?? ServiceSettings;
            }
        }

        internal protected TService GetServiceInstanceOfTypeUID(string typeUID)
        {
            return GetDataOfID(typeUID) ?? DefaultService;
        }

        public virtual void RefreshSettings() { }

        public void LoadSettings(TSettings settings)
        {
            ServiceSettings = settings;
        }

        internal protected TService GetServiceOfNode(NodeData node)
        {
            return GetServiceInstanceOfTypeUID(node.TypeUID);
        }

        internal protected TContext GetContextOfNode(NodeData node, LocalServiceParam localParam)
            => GetContextOfNode(node, localParam, ServiceSettings);

        internal protected TContext GetContextOfNode(NodeData node, LocalServiceParam localParam, TSettings serviceSettings)
        {
            var service = GetServiceInstanceOfTypeUID(node.TypeUID);
            return service.BuildContextForNode(node, localParam, serviceSettings);
        }
    }

    public interface INodeServiceProvider { }

    internal class DefaultNodeServiceProvider : NodeServiceProvider<DefaultNodeServiceProvider, DefaultNodeService, DefaultNodeContext, ServiceExtraSettingsBase> 
    {
        private static readonly DefaultNodeService _default = new();

        protected override DefaultNodeService DefaultService => _default;
    }
}
