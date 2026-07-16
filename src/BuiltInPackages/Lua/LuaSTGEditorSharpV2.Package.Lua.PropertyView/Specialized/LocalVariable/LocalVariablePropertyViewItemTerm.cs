using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.LocalVariable
{
    [Inject(ServiceLifetime.Transient)]
    public class LocalVariablePropertyViewItemTerm(IServiceProvider serviceProvider)
        : IMultipleFieldPropertyItemTerm<VariableDefinition>
    {
        [JsonProperty] public NodePropertyCapture? NameRule { get; set; }
        [JsonProperty] public NodePropertyCapture? ValueRule { get; set; }
        [JsonProperty] public PropertyViewEditorType? NameValueEditor { get; set; }

        public IReadOnlyList<PropertyItemViewModelBase> GetViewModel(EditorNode nodeData, PropertyViewContext context, int count)
        {
            var token = new NodePropertyAccessToken(serviceProvider, nodeData.Source, context);
            var source = new PropertySource(nodeData, token);
            var factory = serviceProvider.GetRequiredService<VariableDefinitionPropertyItemViewModelFactory>();
            List<PropertyItemViewModelBase> properties = [];
            for (int i = 0; i < count; i++)
            {
                properties.Add(factory.Create([source], this, i, NameValueEditor, context.LocalParam));
            }
            return properties;
        }
    }
}
