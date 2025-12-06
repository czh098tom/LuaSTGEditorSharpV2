using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using LuaSTGEditorSharpV2.ResourceDictionaryService;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Stubble.Core.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.PropertyView
{
    [PackedServiceProvider]
    [ServiceName("PropertyView"), ServiceShortName("prop")]
    public class PropertyViewServiceProvider
        : ContextualNodeServiceProvider<PropertyViewServiceBase, PropertyViewContext, PropertyViewServiceSettings>
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

        public override sealed PropertyViewContext GetEmptyContext(LocalServiceParam localSettings
            , PropertyViewServiceSettings serviceSettings)
        {
            return new PropertyViewContext(ServiceProvider, localSettings, serviceSettings);
        }

        public IReadOnlyList<PropertyTabViewModel> GetPropertyViewModelOfNode(EditorNode nodeData
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
        public IReadOnlyList<PropertyTabViewModel> GetPropertyViewModelOfNode(EditorNode nodeData
            , LocalServiceParam localParam, PropertyViewServiceSettings serviceSettings)
        {
            var ctx = GetContextOfNode(nodeData.Source, localParam, serviceSettings);
            return GetPropertyViewModelOfNode(nodeData, ctx);
        }

        public IReadOnlyList<PropertyTabViewModel> GetPropertyViewModelOfNode(EditorNode nodeData, PropertyViewContext ctx)
        {
            var list = new List<PropertyTabViewModel>();
            list.AddRange(GetServiceOfNode(nodeData.Source).ResolvePropertyViewModelOfNode(nodeData, ctx));
            list.Add(CreateDefaultViewModel(nodeData, ctx));
            return list;
        }

        public IReadOnlyList<PropertyTabViewModel> GetPropertyViewModelOfMultipleNodes(EditorNode[] nodeData
            , LocalServiceParam localParam)
        {
            return [CreateDefaultViewModelForNodes(nodeData, localParam)];
        }

        private PropertyTabViewModel CreateDefaultViewModel(EditorNode nodeData, PropertyViewContext context)
        {
            List<PropertyItemViewModelBase> result = new(nodeData.Source.Properties.Count);
            foreach (var prop in nodeData.Source.Properties)
            {
                var vm = ServiceProvider.GetRequiredService<BasicPropertyItemViewModelFactory>()
                    .Create([nodeData], prop.Key, BatchEditStatus.AllSame, context.LocalParam);
                vm.Name = prop.Key;
                vm.Value = prop.Value;
                result.Add(vm);
            }
            PropertyTabViewModel tab = new(true)
            {
                Caption = NativeViewI18NCaption
            };
            result.ForEach(tab.Properties.Add);
            return tab;
        }

        private PropertyTabViewModel CreateDefaultViewModelForNodes(IEnumerable<EditorNode> nodes, LocalServiceParam localServiceParam)
        {
            var firstType = nodes.First().Source.TypeUID;
            if (nodes.Any(n => n.Source.TypeUID != firstType))
            {
                return new PropertyTabViewModel(true)
                {
                    Caption = NativeViewI18NCaption
                };
            }

            List<PropertyItemViewModelBase> result = [];
            var nodeList = nodes.ToList();
            var props = nodes.SelectMany(n => n.Source.Properties, (n, p) => (node: n, prop: p))
                .GroupBy(p => p.prop.Key);
            foreach (var gp in props)
            {
                var value = gp.First().prop.Value;
                var allSame = gp.All(t => t.prop.Value == value);
                var vm = ServiceProvider.GetRequiredService<BasicPropertyItemViewModelFactory>()
                    .Create(nodeList, gp.Key, allSame ? BatchEditStatus.AllSame : BatchEditStatus.SomeDifferent, localServiceParam);
                vm.Name = gp.Key;
                vm.Value = allSame ? value : string.Empty;
                result.Add(vm);
            }
            PropertyTabViewModel tab = new(true)
            {
                Caption = NativeViewI18NCaption
            };
            result.ForEach(tab.Properties.Add);
            return tab;
        }
    }
}
