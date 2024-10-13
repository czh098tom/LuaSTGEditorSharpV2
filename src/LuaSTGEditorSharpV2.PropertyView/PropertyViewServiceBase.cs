using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using LuaSTGEditorSharpV2.PropertyView.Configurable;

namespace LuaSTGEditorSharpV2.PropertyView
{
    /// <summary>
    /// Provide functionality of presenting and manipulating <see cref="NodeData"/> properties.
    /// </summary>
    public class PropertyViewServiceBase(PropertyViewServiceProvider nodeServiceProvider, IServiceProvider serviceProvider)
        : CompactNodeService<PropertyViewServiceProvider, PropertyViewServiceBase, PropertyViewContext, PropertyViewServiceSettings>(nodeServiceProvider, serviceProvider)
    {
        [JsonProperty]
        public PropertyTabTermBase[] Tabs { get; private set; } = [];

        public override sealed PropertyViewContext GetEmptyContext(LocalServiceParam localSettings
            , PropertyViewServiceSettings serviceSettings)
        {
            return new PropertyViewContext(ServiceProvider, localSettings, serviceSettings);
        }

        /// <summary>
        /// Obtain a list of <see cref="PropertyTabViewModel"/> according to data source with 
        /// same TypeUID for providing properties to edit.
        /// </summary>
        /// <param name="nodeData"> The data source with the same TypeUID. </param>
        /// <returns></returns>
        internal protected IReadOnlyList<PropertyTabViewModel> ResolvePropertyViewModelOfNode(NodeData nodeData
            , PropertyViewContext context)
        {
            List<PropertyTabViewModel> propertyTabViewModels = [];
            for (int i = 0; i < Tabs.Length; i++)
            {
                propertyTabViewModels.Add(Tabs[i].GetPropertyTabViewModel(nodeData, context));
            }
            return propertyTabViewModels;
        }
    }
}
