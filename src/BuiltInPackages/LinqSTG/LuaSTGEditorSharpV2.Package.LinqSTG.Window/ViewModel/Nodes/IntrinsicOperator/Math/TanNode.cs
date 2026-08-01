using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using System;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator.Math
{
    public class TanNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel InputXEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputX { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public TanNode()
        {
            InputX = LinqSTGNodeInputViewModel.Float("X", InputXEditor);
            OutputValue = LinqSTGNodeOutputViewModel.Float("Value");
            AddInput("x", InputX);
            AddOutput("value", OutputValue);
            AddEditor("x", InputXEditor);
            Name = "Tan";
            TitleColor = NodeColors.Operator;
            OutputValue.Value = InputX.ValueChanged
                .Select(x => Contextual.Create(dict => MathF.Tan((x?.Invoke(dict) ?? 0f) * MathF.PI / 180f)));
        }
    }
}
