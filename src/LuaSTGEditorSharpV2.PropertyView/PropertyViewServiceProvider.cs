﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView
{
    [ServiceName("PropertyView"), ServiceShortName("prop")]
    public class PropertyViewServiceProvider
        : NodeServiceProvider<PropertyViewServiceProvider, PropertyViewServiceBase, PropertyViewContext, PropertyViewServiceSettings>
    {
        private static PropertyViewServiceBase _defaultService = new();

        private static readonly string _nativeViewI18NKey = "native_view";
        private static readonly string _defaultViewI18NKey = "default_view";

        public static string NativeViewI18NCaption => LocalizedResourceHost.GetString(_nativeViewI18NKey
            , typeof(PropertyViewServiceBase).Assembly) ?? "Native_View";
        public static string DefaultViewI18NCaption => LocalizedResourceHost.GetString(_defaultViewI18NKey
            , typeof(PropertyViewServiceBase).Assembly) ?? "Default_View";

        protected override PropertyViewServiceBase DefaultService => _defaultService;

        public IReadOnlyList<string> ResourceDictUris => _resourceDictUris;

        private readonly List<string> _resourceDictUris =
            ["pack://application:,,,/LuaSTGEditorSharpV2.PropertyView;component/PropertyView.xaml"];

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
            var list = new List<PropertyTabViewModel>();
            list.AddRange(GetServiceOfNode(nodeData).ResolvePropertyViewModelOfNode(nodeData, ctx));
            list.Add(CreateDefaultViewModel(nodeData));
            return list;
        }

        public CommandBase GetCommandOfEditingNode(NodeData nodeData,
            LocalServiceParam localParams, IReadOnlyList<PropertyTabViewModel> propertyList,
            int tabIndex, int itemIndex, string edited)
            => GetCommandOfEditingNode(nodeData, localParams, ServiceSettings, propertyList
                , tabIndex, itemIndex, edited);

        /// <summary>
        /// Obtain a command which manipulate target <see cref="NodeData"/> by infomation from UI.
        /// </summary>
        /// <param name="nodeData"> The data source. </param>
        /// <param name="propertyList"> 
        /// The <see cref="PropertyItemViewModel"/> generated by <see cref="GetPropertyViewModelOfNode"/>. 
        /// </param>
        /// <param name="localParams"> The local params for executing the service. </param>
        /// <param name="tabIndex"> Index of tab in <see cref="PropertyTabViewModel"/>s. </param>
        /// <param name="itemIndex"> Index of item in <see cref="PropertyItemViewModel"/>s. </param>
        /// <param name="edited"> The <see cref="string"/> as edit result. </param>
        /// <returns></returns>
        public CommandBase GetCommandOfEditingNode(NodeData nodeData,
            LocalServiceParam localParams, PropertyViewServiceSettings serviceSettings,
            IReadOnlyList<PropertyTabViewModel> propertyList, int tabIndex, int itemIndex,
            string edited)
        {
            if (tabIndex == propertyList.Count - 1)
                return ResolveNativeEditing(nodeData, propertyList, itemIndex, edited);
            var otherProperties = new List<PropertyTabViewModel>(propertyList.Take(propertyList.Count - 1));
            return GetServiceOfNode(nodeData).ResolveCommandOfEditingNode(nodeData,
                GetContextOfNode(nodeData, localParams, serviceSettings),
                otherProperties, tabIndex, itemIndex, edited);
        }

        private PropertyTabViewModel CreateDefaultViewModel(NodeData nodeData)
        {
            List<PropertyItemViewModel> result = new(nodeData.Properties.Count);
            foreach (var prop in nodeData.Properties)
            {
                result.Add(new PropertyItemViewModel(prop.Key, prop.Value));
            }
            PropertyTabViewModel tab = new()
            {
                Caption = NativeViewI18NCaption
            };
            result.ForEach(tab.Properties.Add);
            return tab;
        }

        public void AddResourceDictUri(string uri)
        {
            _resourceDictUris.Add(uri);
        }

        public CommandBase ResolveNativeEditing(NodeData nodeData,
            IReadOnlyList<PropertyTabViewModel> propertyList,
            int itemIndex, string edited)
        {
            return EditPropertyCommand.CreateEditCommandOnDemand(nodeData,
                propertyList[^1].Properties[itemIndex].Name, edited);
        }
    }
}
