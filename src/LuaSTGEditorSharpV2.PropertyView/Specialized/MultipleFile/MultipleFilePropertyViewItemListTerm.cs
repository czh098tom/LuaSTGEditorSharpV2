using System;
using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Resources;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.PropertyView.Specialized.MultipleFile;

[Inject(ServiceLifetime.Transient)]
public class MultipleFilePropertyViewItemListTerm(IServiceProvider serviceProvider)
    : PropertyItemListTermBase(serviceProvider)
{
    [JsonProperty]
    public FileListMapping Files { get; private set; } = new();

    [JsonProperty]
    public PropertyViewEditorType? Editor { get; private set; } = new("multiplefileEntry");

    public override IReadOnlyList<PropertyItemViewModelBase> GetViewModels(
        IReadOnlyList<EditorNode> nodes, PropertyViewContext context, int count)
    {
        var items = new List<PropertyItemViewModelBase>();
        for (var index = 0; index < count; index++)
        {
            var term = new ItemTerm(ServiceProvider, Editor)
            {
                Files = Files,
                Mapping = new FileListItemMapping(
                    index, Files.Source.Format(index), Files.Destination.Format(index)),
            };
            items.Add(term.GetViewModel(nodes, context));
        }
        return items;
    }

    public sealed class ItemTerm : PropertyItemTermBase
    {
        public ItemTerm(IServiceProvider serviceProvider, PropertyViewEditorType? editor) : base(serviceProvider)
        {
            Editor = editor;
        }

        public FileListMapping Files { get; init; } = null!;
        public FileListItemMapping Mapping { get; init; } = null!;

        public override PropertyItemViewModelBase GetViewModel(
            IReadOnlyList<EditorNode> nodes, PropertyViewContext context)
        {
            return ServiceProvider.GetRequiredService<
                IPropertyItemViewModelFactory<MultipleFileEntryViewModel, ItemTerm>>()
                .Create(nodes, this, Editor, context);
        }
    }
}
