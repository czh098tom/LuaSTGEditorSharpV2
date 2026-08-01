using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using System;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator.Math
{
    public class ClampNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel InputXEditor { get; } = new();
        public FloatValueEditorViewModel InputMinEditor { get; } = new();
        public FloatValueEditorViewModel InputMaxEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputX { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputMin { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputMax { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public ClampNode()
        {
            InputX = LinqSTGNodeInputViewModel.Float("X", InputXEditor);
            InputMin = LinqSTGNodeInputViewModel.Float("Min", InputMinEditor);
            InputMax = LinqSTGNodeInputViewModel.Float("Max", InputMaxEditor);
            OutputValue = LinqSTGNodeOutputViewModel.Float("Value");
            AddInput("x", InputX);
            AddInput("min", InputMin);
            AddInput("max", InputMax);
            AddOutput("value", OutputValue);
            AddEditor("x", InputXEditor);
            AddEditor("min", InputMinEditor);
            AddEditor("max", InputMaxEditor);
            Name = "Clamp";
            TitleColor = NodeColors.Operator;
            OutputValue.Value = InputX.ValueChanged
                .CombineLatest(InputMin.ValueChanged, InputMax.ValueChanged,
                    (x, min, max) => Contextual.Create(dict => MathF.Min(MathF.Max(x?.Invoke(dict) ?? 0f, min?.Invoke(dict) ?? 0f), max?.Invoke(dict) ?? 0f)));
        }
    }
}
