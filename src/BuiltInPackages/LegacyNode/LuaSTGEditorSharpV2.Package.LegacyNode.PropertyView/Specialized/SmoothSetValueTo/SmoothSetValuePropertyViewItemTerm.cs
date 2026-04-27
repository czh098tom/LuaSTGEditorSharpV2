using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.PropertyView.Specialized.SmoothSetValueTo
{
    [Inject(ServiceLifetime.Transient)]
    public class SmoothSetValuePropertyViewItemTerm(IServiceProvider serviceProvider)
        : IMultipleFieldPropertyItemTerm<SmoothSetValueDefinition>
    {
        [JsonProperty] public NodePropertyCapture? VariableNameRule { get; set; }
		[JsonProperty] public NodePropertyCapture? TargetValueRule { get; set; }
        [JsonProperty] public NodePropertyCapture? InterpolationModeRule { get; set; }
		[JsonProperty] public NodePropertyCapture? ModificationModeRule { get; set; }
		[JsonProperty] public PropertyViewEditorType? Editor { get; set; }

        public IReadOnlyList<PropertyItemViewModelBase> GetViewModel(EditorNode nodeData, PropertyViewContext context, int count)
        {
            var token = new NodePropertyAccessToken(serviceProvider, nodeData.Source, context);
            List<PropertyItemViewModelBase> properties = [];
            for (int i = 0; i < count; i++)
            {
                object idx = i;
                var vm = serviceProvider.GetRequiredService<SmoothSetValueDefinitionPropertyItemViewModelFactory>()
                    .Create(this, i, nodeData, context.LocalParam);
				vm.Type = Editor;
                vm.SetProxy(
                    VariableNameRule?.CaptureByFormat(token, idx) ?? string.Empty,
                    TargetValueRule?.CaptureByFormat(token, idx) ?? string.Empty,
                    InterpolationModeRule?.CaptureByFormat(token, idx) ?? string.Empty,
                    ModificationModeRule?.CaptureByFormat(token, idx) ?? string.Empty);
                properties.Add(vm);
            }
            return properties;
        }
    }
}
