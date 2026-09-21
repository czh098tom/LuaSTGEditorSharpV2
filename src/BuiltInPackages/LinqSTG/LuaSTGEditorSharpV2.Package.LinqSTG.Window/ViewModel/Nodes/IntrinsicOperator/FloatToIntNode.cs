using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator
{
    [NodeCreationMenu("Operator", TitleKey = "linqstg_window_node_floatToInt", Order = 11)]
    public class FloatToIntNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputFloat { get; }
        public LinqSTGNodeOutputViewModel<Contextual<int>> OutputInt { get; }

        public FloatToIntNode()
        {
            InputFloat = LinqSTGNodeInputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_float);
            OutputInt = LinqSTGNodeOutputViewModel.Int(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_int);

            AddInput("float", InputFloat);
            AddOutput("int", OutputInt);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_floatToInt;
            TitleColor = NodeColors.Operator;

            OutputInt.Value = InputFloat.ValueChanged
                .Select(floatValue => Contextual.Create(dict => NumericConversion.ToInt32(floatValue?.Invoke(dict) ?? 0), OutputInt));
        }
    }
}
