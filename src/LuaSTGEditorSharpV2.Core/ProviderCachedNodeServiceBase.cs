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
    public abstract class ProviderCachedNodeServiceBase<TNodeServiceProvider>(TNodeServiceProvider nodeServiceProvider, IServiceProvider serviceProvider) : NodeServiceBase(serviceProvider)
        where TNodeServiceProvider : class
    {
        [JsonIgnore]
        protected TNodeServiceProvider NodeServiceProvider { get; private set; } = nodeServiceProvider;
    }

    internal class DefaultNodeService(DefaultNodeServiceProvider nodeServiceProvider, IServiceProvider serviceProvider) 
        : ProviderCachedNodeServiceBase<DefaultNodeServiceProvider>(nodeServiceProvider, serviceProvider)
    {
    }
}
