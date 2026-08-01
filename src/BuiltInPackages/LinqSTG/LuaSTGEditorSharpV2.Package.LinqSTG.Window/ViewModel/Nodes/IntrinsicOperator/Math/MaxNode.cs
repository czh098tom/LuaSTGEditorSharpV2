using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using System;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator.Math
{
    public class MaxNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel InputAEditor { get; } = new();
        public FloatValueEditorViewModel InputBEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputA { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputB { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public MaxNode()
        {
            InputA = LinqSTGNodeInputViewModel.Float("A", InputAEditor);
            InputB = LinqSTGNodeInputViewModel.Float("B", InputBEditor);
            OutputValue = LinqSTGNodeOutputViewModel.Float("Value");
            AddInput("a", InputA);
            AddInput("b", InputB);
            AddOutput("value", OutputValue);
            AddEditor("a", InputAEditor);
            AddEditor("b", InputBEditor);
            Name = "Max";
            TitleColor = NodeColors.Operator;
            OutputValue.Value = InputA.ValueChanged
                .CombineLatest(InputB.ValueChanged,
                    (a, b) => Contextual.Create(dict => MathF.Max(a?.Invoke(dict) ?? 0f, b?.Invoke(dict) ?? 0f)));
        }
    }
}
