using System;
using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Resources;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.PropertyView.Specialized.MultipleFile;

[JsonTypeShortName(typeof(IPropertyItemTerm), "MultipleFile")]
[JsonTypeShortName(typeof(PropertyItemTerm), "MultipleFile")]
public class MultipleFileCountPropertyItemTerm : PropertyItemTerm
{
    [JsonProperty]
    public FileListMapping Files { get; private set; } = new();

    public MultipleFileCountPropertyItemTerm(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Editor = new PropertyViewEditorType("multiplefile");
    }

    public override PropertyItemViewModelBase GetViewModel(
        IReadOnlyList<EditorNode> nodes, PropertyViewContext context)
    {
        return ServiceProvider.GetRequiredService<
            IPropertyItemViewModelFactory<MultipleFileCountPropertyItemViewModel, MultipleFileCountPropertyItemTerm>>()
            .Create(nodes, this, Editor, context);
    }
}
