using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    [JsonTypeShortName(typeof(IPropertyItemTerm), "CollectionCount")]
    [JsonTypeShortName(typeof(PropertyItemTerm), "CollectionCount")]
    public class CollectionCountPropertyItemTerm : PropertyItemTerm
    {
        public CollectionCountPropertyItemTerm(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Editor = new PropertyViewEditorType("collectionCount");
        }

        public override PropertyItemViewModelBase GetViewModel(NodeData nodeData, PropertyViewContext context)
        {
            return GetViewModelImpl<CollectionCountPropertyItemViewModelFactory, CollectionCountPropertyItemViewModel>(nodeData, context);
        }
    }
}
