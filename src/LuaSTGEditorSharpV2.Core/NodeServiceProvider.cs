using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core
{
    public abstract class NodeServiceProvider<TService> : PackedDataProviderServiceBase<TService>, INodeServiceProvider
        where TService : NodeServiceBase
    {
        protected abstract TService DefaultService { get; }

        internal protected TService GetServiceInstanceOfTypeUID(string typeUID)
        {
            return GetDataOfID(typeUID) ?? DefaultService;
        }

        internal protected TService GetServiceOfNode(NodeData node)
        {
            return GetServiceInstanceOfTypeUID(node.TypeUID);
        }
    }

    public interface INodeServiceProvider { }
}
