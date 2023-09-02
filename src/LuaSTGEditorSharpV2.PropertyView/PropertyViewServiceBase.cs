using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView
{
    [ServiceName("PropertyView"), ServiceShortName("prop")]
    public class PropertyViewServiceBase 
        : NodeService<PropertyViewServiceBase, PropertyViewContext, PropertyViewServiceSettings>
    {
        private static readonly PropertyViewServiceBase _defaultService = new();

        static PropertyViewServiceBase()
        {
            _defaultServiceGetter = () => _defaultService;
        }

        public static IReadOnlyList<PropertyViewModel> GetPropertyViewModelOfNode(NodeData nodeData, int subtype = 0)
        {
            return GetServiceOfNode(nodeData).ResolvePropertyViewModelOfNode(nodeData, subtype);
        }

        public static CommandBase GetCommandOfEditingNode(NodeData nodeData
            , IReadOnlyList<PropertyViewModel> propertyList, int index, string edited, int subtype = 0)
        {
            return GetServiceOfNode(nodeData).ResolveCommandOfEditingNode(nodeData, propertyList
                , index, edited, subtype);
        }

        public override sealed PropertyViewContext GetEmptyContext(LocalSettings localSettings)
        {
            return new PropertyViewContext(localSettings);
        }

        protected virtual IReadOnlyList<PropertyViewModel> ResolvePropertyViewModelOfNode(NodeData nodeData
            , int subtype = 0)
        {
            List<PropertyViewModel> result = new(nodeData.Properties.Count);
            foreach (var prop in nodeData.Properties)
            {
                result.Add(new PropertyViewModel(prop.Key, prop.Value));
            }
            return result;
        }

        protected virtual CommandBase ResolveCommandOfEditingNode(NodeData nodeData
            , IReadOnlyList<PropertyViewModel> propertyList, int index, string edited, int subtype = 0)
        {
            return EditPropertyCommand.CreateEditCommandOnDemand(nodeData, propertyList[index].Name, edited);
        }
    }
}
