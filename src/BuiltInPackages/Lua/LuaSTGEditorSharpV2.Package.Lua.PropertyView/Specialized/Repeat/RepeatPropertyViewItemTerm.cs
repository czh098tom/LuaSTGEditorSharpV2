using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.Package.Lua.PropertyView.Specialized.Repeat
{
    [Inject(ServiceLifetime.Transient)]
    public class RepeatPropertyViewItemTerm(IServiceProvider serviceProvider) 
        : IMultipleFieldPropertyItemTerm<RepeatVariableDefinition>
    {
        [JsonProperty] public NodePropertyCapture? NameRule { get; set; }
        [JsonProperty] public NodePropertyCapture? InitRule { get; set; }
        [JsonProperty] public NodePropertyCapture? IncrementRule { get; set; }
        [JsonProperty] public PropertyViewEditorType? NameValueEditor { get; set; }

        public IReadOnlyList<PropertyItemViewModelBase> GetViewModel(EditorNode nodeData, PropertyViewContext context, int count)
        {
            var token = new NodePropertyAccessToken(serviceProvider, nodeData.Source, context);
            var source = new PropertySource(nodeData, token);
            var factory = serviceProvider.GetRequiredService<RepeatVariableDefinitionPropertyItemViewModelFactory>();
            List<PropertyItemViewModelBase> properties = [];
            for (int i = 0; i < count; i++)
            {
                properties.Add(factory.Create([source], this, i, NameValueEditor, context.LocalParam));
            }
            return properties;
        }
    }
}
