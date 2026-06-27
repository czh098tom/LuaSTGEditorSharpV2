using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data
{
    public class Vector2Node : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel XEditor { get; } = new();
        public FloatValueEditorViewModel YEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputX { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputY { get; }
        public LinqSTGNodeOutputViewModel<Contextual<Vector2>> OutputVector2 { get; }

        public Vector2Node()
        {
            InputX = LinqSTGNodeInputViewModel.Float("X", XEditor);
            InputY = LinqSTGNodeInputViewModel.Float("Y", YEditor);
            OutputVector2 = LinqSTGNodeOutputViewModel.Vector2("Vector2");

            AddInput("x", InputX);
            AddInput("y", InputY);
            AddOutput("vector2", OutputVector2);
            AddEditor("x", XEditor);
            AddEditor("y", YEditor);

            Name = "Vector2";
            TitleColor = NodeColors.Data;

            OutputVector2.Value = InputX.ValueChanged
                .CombineLatest(InputY.ValueChanged, (x, y) => Contextual.Create(dict =>
                    new Vector2(x?.Invoke(dict) ?? 0f, y?.Invoke(dict) ?? 0f)));
        }
    }
}
