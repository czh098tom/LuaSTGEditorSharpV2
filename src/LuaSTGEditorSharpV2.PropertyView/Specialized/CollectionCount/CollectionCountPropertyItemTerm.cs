using System;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView.Specialized.CollectionCount
{
    [JsonTypeShortName(typeof(IPropertyItemTerm), "CollectionCount")]
    [JsonTypeShortName(typeof(PropertyItemTerm), "CollectionCount")]
    public class CollectionCountPropertyItemTerm : PropertyItemTerm
    {
        public CollectionCountPropertyItemTerm(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Editor = new PropertyViewEditorType("collectionCount");
        }

        public override PropertyItemViewModelBase GetViewModel(EditorNode nodeData, PropertyViewContext context)
        {
            return GetViewModelImpl<CollectionCountPropertyItemViewModel, PropertyItemTerm>(nodeData, context, this, Editor);
        }
    }
}
