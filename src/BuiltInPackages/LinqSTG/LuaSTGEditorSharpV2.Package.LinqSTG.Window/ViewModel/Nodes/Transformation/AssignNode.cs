using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using log4net.Repository.Hierarchy;
using Microsoft.VisualBasic.ApplicationServices;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Transformation
{
    [NodeCreationMenu("Assignment", TitleKey = "linqstg_window_node_assign", Order = 0)]
    public class AssignNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel InputValueEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<Parameter>?> InputTransformation { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputValue { get; }
        public LinqSTGNodeInputViewModel<Contextual<string>?> InputKey { get; }
        public LinqSTGNodeOutputViewModel<Contextual<Parameter>> OutputTransformation { get; }

        public AssignNode()
        {
            InputTransformation = LinqSTGNodeInputViewModel.Transformation(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_transformation);
            InputValue = LinqSTGNodeInputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_value, InputValueEditor);
            InputKey = LinqSTGNodeInputViewModel.String(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_key);
            OutputTransformation = LinqSTGNodeOutputViewModel.Transformation(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_transformation);

            AddInput("transformation", InputTransformation);
            AddInput("value", InputValue);
            AddInput("key", InputKey);
            AddOutput("transformation", OutputTransformation);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_assign;

            TitleColor = NodeColors.Assignment;

            OutputTransformation.Value = InputTransformation.ValueChanged
                .CombineLatest(InputKey.ValueChanged, InputValue.ValueChanged,
                    (trans, key, value) => Contextual.Create(dict =>
                    {
                        dict = trans?.Invoke(dict) ?? dict;

                        var parameter = new Parameter(dict);
                        var inputKey = key?.Invoke(dict) ?? "Key";
                        var inputValue = value?.Invoke(dict) ?? 0;

                        parameter.Floats[inputKey] = inputValue;
                        return parameter;
                    }));
        }
    }
}
