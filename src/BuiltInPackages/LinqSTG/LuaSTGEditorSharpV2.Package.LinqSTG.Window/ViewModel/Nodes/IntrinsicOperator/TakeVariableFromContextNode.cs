using DynamicData;
using LinqSTG;
using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator
{
    [NodeCreationMenu("Operator/Context", TitleKey = "linqstg_window_node_takeVariable", Order = 0)]
    public class TakeVariableFromContextNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<string>?> InputValue { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public TakeVariableFromContextNode()
        {
            InputValue = LinqSTGNodeInputViewModel.String(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_key);
            OutputValue = LinqSTGNodeOutputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_value);

            AddInput("key", InputValue);
            AddOutput("value", OutputValue);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_takeVariable;

            TitleColor = NodeColors.Operator;

            OutputValue.Value = InputValue.ValueChanged
                .Select(s => Contextual.Create(dict => dict.Floats.GetValueOrDefault(s?.Invoke(dict) ?? string.Empty, 0f)));
        }
    }
}
