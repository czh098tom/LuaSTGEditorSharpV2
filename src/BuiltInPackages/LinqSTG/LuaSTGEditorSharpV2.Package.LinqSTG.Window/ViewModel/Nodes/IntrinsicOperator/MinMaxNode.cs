using DynamicData;
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
    public class MinMaxNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel InputLowerBoundEditor { get; } = new();
        public FloatValueEditorViewModel InpuUpperBoundEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputValue { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputLowerBound { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InpuUpperBound { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public MinMaxNode()
        {
            InputValue = LinqSTGNodeInputViewModel.Float("Value", InputLowerBoundEditor);
            InputLowerBound = LinqSTGNodeInputViewModel.Float("Lower Bound", InputLowerBoundEditor);
            InpuUpperBound = LinqSTGNodeInputViewModel.Float("Upper Bound", InpuUpperBoundEditor);
            OutputValue = LinqSTGNodeOutputViewModel.Float("Value");

            AddInput("input_value", InputValue);
            AddInput("lower_bound", InputLowerBound);
            AddInput("upper_bound", InpuUpperBound);
            AddOutput("output_value", OutputValue);
            AddEditor("lower_bound", InputLowerBoundEditor);
            AddEditor("upper_bound", InpuUpperBoundEditor);

            Name = "MinMax";

            TitleColor = NodeColors.Operator;

            OutputValue.Value = InputValue.ValueChanged
                .CombineLatest(InputLowerBound.ValueChanged, InpuUpperBound.ValueChanged,
                    (input, lower, upper) => Contextual.Create(dict => (input?.Invoke(dict) ?? 0f)
                        .MinMax(lower?.Invoke(dict) ?? 0f, upper?.Invoke(dict) ?? 0f)));
        }
    }
}
