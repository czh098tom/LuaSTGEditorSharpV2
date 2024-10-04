using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    [Inject(ServiceLifetime.Transient)]
    public abstract class PropertyViewTabTermBase(IServiceProvider serviceProvider, 
        PropertyViewServiceProvider propertyViewServiceProvider)
    {
        [JsonProperty] public LocalizableString? Caption { get; private set; }

        protected IServiceProvider ServiceProvider { get; private set; } = serviceProvider;
        protected PropertyViewServiceProvider PropertyViewServiceProvider { get; private set; } = propertyViewServiceProvider;

        public abstract PropertyTabViewModel GetPropertyTabViewModel(NodeData nodeData, PropertyViewContext context);
    }
}
