using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using LuaSTGEditorSharpV2.ResourceDictionaryService;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace LuaSTGEditorSharpV2.PropertyView
{
    [PackedServiceProvider]
    [ServiceName("PropertyView"), ServiceShortName("prop")]
    public class PropertyViewServiceProvider
        : CompactNodeServiceProvider<PropertyViewServiceProvider, PropertyViewServiceBase, PropertyViewContext, PropertyViewServiceSettings>
    {
        private static readonly string _nativeViewI18NKey = "native_view";
        private static readonly string _defaultViewI18NKey = "default_view";

        public string NativeViewI18NCaption => ServiceProvider
            .GetRequiredService<LocalizationService>()
            .GetString(_nativeViewI18NKey, typeof(PropertyViewServiceBase).Assembly);
        public string DefaultViewI18NCaption => ServiceProvider
            .GetRequiredService<LocalizationService>()
            .GetString(_defaultViewI18NKey, typeof(PropertyViewServiceBase).Assembly);

        private readonly PropertyViewServiceBase _defaultService;

        public PropertyViewServiceProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _defaultService = new(this, serviceProvider);
        }

        protected override PropertyViewServiceBase DefaultService => _defaultService;

        public IReadOnlyList<PropertyTabViewModel> GetPropertyViewModelOfNode(NodeData nodeData
            , LocalServiceParam localParam)
            => GetPropertyViewModelOfNode(nodeData, localParam, ServiceSettings);

        /// <summary>
        /// Obtain a list of <see cref="PropertyTabViewModel"/> according to data source for providing properties to edit. 
        /// </summary>
        /// <param name="nodeData"> The data source. </param>
        /// <param name="localParam"> Th local param for this action. </param>
        /// <param name="serviceSettings"> The <see cref="PropertyViewServiceSettings"/> for this action. </param>
        /// <param name="subtype"></param>
        /// <returns></returns>
        public IReadOnlyList<PropertyTabViewModel> GetPropertyViewModelOfNode(NodeData nodeData
            , LocalServiceParam localParam, PropertyViewServiceSettings serviceSettings)
        {
            var ctx = GetContextOfNode(nodeData, localParam, serviceSettings);
            return GetPropertyViewModelOfNode(nodeData, ctx);
        }

        public IReadOnlyList<PropertyTabViewModel> GetPropertyViewModelOfNode(NodeData nodeData, PropertyViewContext ctx)
        {
            var list = new List<PropertyTabViewModel>();
            list.AddRange(GetServiceOfNode(nodeData).ResolvePropertyViewModelOfNode(nodeData, ctx));
            list.Add(CreateDefaultViewModel(nodeData, ctx));
            return list;
        }

        private PropertyTabViewModel CreateDefaultViewModel(NodeData nodeData, PropertyViewContext context)
        {
            List<PropertyItemViewModelBase> result = new(nodeData.Properties.Count);
            foreach (var prop in nodeData.Properties)
            {
                var vm = ServiceProvider.GetRequiredService<BasicPropertyItemViewModelFactory>()
                    .Create(nodeData, context.LocalParam, prop.Key);
                vm.Name = prop.Key;
                vm.Value = prop.Value;
                result.Add(vm);
            }
            PropertyTabViewModel tab = new()
            {
                Caption = NativeViewI18NCaption
            };
            result.ForEach(tab.Properties.Add);
            return tab;
        }
    }
}
