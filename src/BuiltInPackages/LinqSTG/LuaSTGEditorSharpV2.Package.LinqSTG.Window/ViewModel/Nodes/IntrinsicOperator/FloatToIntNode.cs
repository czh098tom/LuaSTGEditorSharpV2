using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator
{
    public class FloatToIntNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputFloat { get; }
        public LinqSTGNodeOutputViewModel<Contextual<int>> OutputInt { get; }

        public FloatToIntNode()
        {
            InputFloat = LinqSTGNodeInputViewModel.Float("Float");
            OutputInt = LinqSTGNodeOutputViewModel.Int("Int");

            AddInput("float", InputFloat);
            AddOutput("int", OutputInt);

            Name = "Float To Int";
            TitleColor = NodeColors.Operator;

            OutputInt.Value = InputFloat.ValueChanged
                .Select(floatValue => Contextual.Create(dict => Convert.ToInt32(floatValue?.Invoke(dict) ?? 0)));
        }
    }
}
