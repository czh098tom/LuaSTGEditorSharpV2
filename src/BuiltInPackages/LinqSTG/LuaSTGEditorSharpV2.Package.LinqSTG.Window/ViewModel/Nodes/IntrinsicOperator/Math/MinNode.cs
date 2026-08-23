using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using System;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator.Math
{
    [NodeCreationMenu("Operator/Math", EnglishTitle = "Min")]
    public class MinNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel InputAEditor { get; } = new();
        public FloatValueEditorViewModel InputBEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputA { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputB { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public MinNode()
        {
            InputA = LinqSTGNodeInputViewModel.Float("A", InputAEditor);
            InputB = LinqSTGNodeInputViewModel.Float("B", InputBEditor);
            OutputValue = LinqSTGNodeOutputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_value);
            AddInput("a", InputA);
            AddInput("b", InputB);
            AddOutput("value", OutputValue);
            AddEditor("a", InputAEditor);
            AddEditor("b", InputBEditor);
            Name = "Min";
            TitleColor = NodeColors.Operator;
            OutputValue.Value = InputA.ValueChanged
                .CombineLatest(InputB.ValueChanged,
                    (a, b) => Contextual.Create(dict => MathF.Min(a?.Invoke(dict) ?? 0f, b?.Invoke(dict) ?? 0f)));
        }
    }
}
