using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using System;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator.Math
{
    [NodeCreationMenu("Operator/Math", EnglishTitle = "Lerp")]
    public class LerpNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel InputAEditor { get; } = new();
        public FloatValueEditorViewModel InputBEditor { get; } = new();
        public FloatValueEditorViewModel InputTEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputA { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputB { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputT { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public LerpNode()
        {
            InputA = LinqSTGNodeInputViewModel.Float("A", InputAEditor);
            InputB = LinqSTGNodeInputViewModel.Float("B", InputBEditor);
            InputT = LinqSTGNodeInputViewModel.Float("T", InputTEditor);
            OutputValue = LinqSTGNodeOutputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_value);
            AddInput("a", InputA);
            AddInput("b", InputB);
            AddInput("t", InputT);
            AddOutput("value", OutputValue);
            AddEditor("a", InputAEditor);
            AddEditor("b", InputBEditor);
            AddEditor("t", InputTEditor);
            Name = "Lerp";
            TitleColor = NodeColors.Operator;
            OutputValue.Value = InputA.ValueChanged
                .CombineLatest(InputB.ValueChanged, InputT.ValueChanged,
                    (a, b, t) => Contextual.Create(dict => (a?.Invoke(dict) ?? 0f) + ((b?.Invoke(dict) ?? 0f) - (a?.Invoke(dict) ?? 0f)) * (t?.Invoke(dict) ?? 0f)));
        }
    }
}
