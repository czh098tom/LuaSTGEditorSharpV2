using DynamicData;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Serialization
{
    public record class NodeModel
        (
            [property: JsonProperty("type", Required = Required.Always)] string AssemblyQualifiedName,
            [property: JsonProperty("x")] double X,
            [property: JsonProperty("y")] double Y,
            [property: JsonProperty("editors", Required = Required.Always)] JObject Editors
        )
    {
        public static NodeModel FromViewModel(LinqSTGNodeViewModel viewModel)
        {
            var typeName = viewModel.GetType().AssemblyQualifiedName 
                ?? throw new InvalidOperationException("ViewModel type does not have an AssemblyQualifiedName.");

            var x = viewModel.Position.X;
            var y = viewModel.Position.Y;
            var editors = new JObject();

            foreach (var editor in viewModel.EditorDict)
            {
                if (editor.Value is IContextualValueEditorViewModel<float> floatEditor)
                {
                    editors[editor.Key] = JToken.FromObject(floatEditor.RawValue);
                }
                else if (editor.Value is IContextualValueEditorViewModel<int> intEditor)
                {
                    editors[editor.Key] = JToken.FromObject(intEditor.RawValue);
                }
                else if (editor.Value is IContextualValueEditorViewModel<string> stringEditor)
                {
                    editors[editor.Key] = JToken.FromObject(stringEditor.RawValue);
                }
            }

            return new(typeName, x, y, editors);
        }

        public LinqSTGNodeViewModel CreateViewModel()
        {
            var type = Type.GetType(AssemblyQualifiedName) 
                ?? throw new InvalidOperationException($"Type '{AssemblyQualifiedName}' not found.");
            var viewModel = (LinqSTGNodeViewModel?)Activator.CreateInstance(type) 
                ?? throw new InvalidOperationException($"Could not create instance of type '{AssemblyQualifiedName}'.");
            viewModel.Position = new(X, Y);

            foreach (var editor in Editors)
            {
                if (viewModel.EditorDict.TryGetValue(editor.Key, out var editorViewModel))
                {
                    if (editorViewModel is IContextualValueEditorViewModel<float> floatContextual)
                    {
                        floatContextual.RawValue = editor.Value?.ToObject<float>()
                            ?? throw new InvalidOperationException($"Could not convert value for editor '{editor.Key}' to float.");
                    }
                    else if (editorViewModel is IContextualValueEditorViewModel<int> intContextual)
                    {
                        intContextual.RawValue = editor.Value?.ToObject<int>()
                            ?? throw new InvalidOperationException($"Could not convert value for editor '{editor.Key}' to int.");
                    }
                    else if (editorViewModel is IContextualValueEditorViewModel<string> stringContextual)
                    {
                        stringContextual.RawValue = editor.Value?.ToObject<string>()
                            ?? throw new InvalidOperationException($"Could not convert value for editor '{editor.Key}' to string.");
                    }
                }
            }

            return viewModel;
        }
    }
}
