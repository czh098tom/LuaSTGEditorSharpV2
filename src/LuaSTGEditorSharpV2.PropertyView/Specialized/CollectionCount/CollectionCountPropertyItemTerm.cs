using System;
using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using Microsoft.Extensions.DependencyInjection;

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

        public override PropertyItemViewModelBase GetViewModel(IReadOnlyList<EditorNode> nodes, PropertyViewContext context)
        {
            var factory = ServiceProvider.GetRequiredService<
                IPropertyItemViewModelFactory<CollectionCountPropertyItemViewModel, PropertyItemTerm>>();
            return factory.Create(nodes, this, Editor, context);
        }
    }
}
