using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using System;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator.Math
{
    public class PowNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel InputBaseEditor { get; } = new();
        public FloatValueEditorViewModel InputExponentEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputBase { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputExponent { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public PowNode()
        {
            InputBase = LinqSTGNodeInputViewModel.Float("Base", InputBaseEditor);
            InputExponent = LinqSTGNodeInputViewModel.Float("Exponent", InputExponentEditor);
            OutputValue = LinqSTGNodeOutputViewModel.Float("Value");
            AddInput("base", InputBase);
            AddInput("exponent", InputExponent);
            AddOutput("value", OutputValue);
            AddEditor("base", InputBaseEditor);
            AddEditor("exponent", InputExponentEditor);
            Name = "Pow";
            TitleColor = NodeColors.Operator;
            OutputValue.Value = InputBase.ValueChanged
                .CombineLatest(InputExponent.ValueChanged,
                    (b, e) => Contextual.Create(dict => MathF.Pow(b?.Invoke(dict) ?? 0f, e?.Invoke(dict) ?? 0f)));
        }
    }
}
