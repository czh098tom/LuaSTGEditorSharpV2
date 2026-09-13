using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator
{
    [NodeCreationMenu("Operator", TitleKey = "linqstg_window_node_intToFloat", Order = 12)]
    public class IntToFloatNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<int>?> InputInt { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputFloat { get; }

        public IntToFloatNode()
        {
            InputInt = LinqSTGNodeInputViewModel.Int(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_int);
            OutputFloat = LinqSTGNodeOutputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_float);

            AddInput("int", InputInt);
            AddOutput("float", OutputFloat);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_intToFloat;
            TitleColor = NodeColors.Operator;

            OutputFloat.Value = InputInt.ValueChanged
                .Select(intValue => Contextual.Create(dict => Convert.ToSingle(intValue?.Invoke(dict) ?? 0)));
        }
    }
}
