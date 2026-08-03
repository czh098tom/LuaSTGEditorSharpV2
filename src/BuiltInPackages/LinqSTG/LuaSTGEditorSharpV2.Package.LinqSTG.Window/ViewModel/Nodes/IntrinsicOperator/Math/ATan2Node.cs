using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using System;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator.Math
{
    public class ATan2Node : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel InputYEditor { get; } = new();
        public FloatValueEditorViewModel InputXEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputY { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputX { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public ATan2Node()
        {
            InputY = LinqSTGNodeInputViewModel.Float("Y", InputYEditor);
            InputX = LinqSTGNodeInputViewModel.Float("X", InputXEditor);
            OutputValue = LinqSTGNodeOutputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_value);
            AddInput("y", InputY);
            AddInput("x", InputX);
            AddOutput("value", OutputValue);
            AddEditor("y", InputYEditor);
            AddEditor("x", InputXEditor);
            Name = "ATan2";
            TitleColor = NodeColors.Operator;
            OutputValue.Value = InputY.ValueChanged
                .CombineLatest(InputX.ValueChanged,
                    (y, x) => Contextual.Create(dict => MathF.Atan2(y?.Invoke(dict) ?? 0f, x?.Invoke(dict) ?? 0f) * 180f / MathF.PI));
        }
    }
}
