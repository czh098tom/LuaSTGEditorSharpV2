using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using global::LinqSTG.Easings;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator
{
    public class Sample01MinMaxNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel InputLowerBoundEditor { get; } = new();
        public FloatValueEditorViewModel InpuUpperBoundEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<Repeater>?> InputRepeater { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputLowerBound { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InpuUpperBound { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public Sample01MinMaxNode()
        {
            InputRepeater = LinqSTGNodeInputViewModel.Repeater("Repeater");
            InputLowerBound = LinqSTGNodeInputViewModel.Float("Lower Bound", InputLowerBoundEditor);
            InpuUpperBound = LinqSTGNodeInputViewModel.Float("Upper Bound", InpuUpperBoundEditor);
            OutputValue = LinqSTGNodeOutputViewModel.Float("Value");

            AddInput("repeater", InputRepeater);
            AddInput("lower_bound", InputLowerBound);
            AddInput("upper_bound", InpuUpperBound);
            AddOutput("value", OutputValue);
            AddEditor("lower_bound", InputLowerBoundEditor);
            AddEditor("upper_bound", InpuUpperBoundEditor);

            Name = "Sample01 MinMax";
            TitleColor = NodeColors.Operator;

            OutputValue.Value = InputRepeater.ValueChanged
                .CombineLatest(InputLowerBound.ValueChanged, InpuUpperBound.ValueChanged,
                    (repeater, lower, upper) => Contextual.Create(dict =>
                        (repeater?.Invoke(dict) ?? RepeaterKey.Default.GetRepeater(dict))
                            .Sample01(IntervalType.HeadClosed)
                            .MinMax(lower?.Invoke(dict) ?? 0f, upper?.Invoke(dict) ?? 0f)));
        }
    }
}
