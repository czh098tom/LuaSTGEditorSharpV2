using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    [Inject(ServiceLifetime.Transient)]
    [JsonUseShortNaming]
    [JsonTypeShortName(typeof(IPropertyItemTerm), "Default")]
    public class PropertyItemTerm(IServiceProvider serviceProvider)
        : PropertyItemTermBase(serviceProvider)
    {
        [JsonProperty] public NodePropertyCapture Mapping { get; private set; } = null!;
        [JsonProperty] public LocalizableString Caption { get; private set; } = new();
        [JsonProperty] public PropertyViewEditorType? Editor { get; protected set; }
        [JsonProperty] public bool Enabled { get; private set; } = true;

        public override PropertyItemViewModelBase GetViewModel(EditorNode nodeData, PropertyViewContext context)
        {
            return GetViewModelImpl<BasicPropertyItemViewModel, PropertyItemTerm>(nodeData, context, this, Editor);
        }
    }
}
