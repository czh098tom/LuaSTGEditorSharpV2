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
    public class Vector2FromRotationDistanceNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel RotationEditor { get; } = new();
        public FloatValueEditorViewModel DistanceEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputRotation { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputDistance { get; }
        public LinqSTGNodeOutputViewModel<Contextual<Vector2>> OutputVector2 { get; }

        public Vector2FromRotationDistanceNode()
        {
            InputRotation = LinqSTGNodeInputViewModel.Float("Rotation", RotationEditor);
            InputDistance = LinqSTGNodeInputViewModel.Float("Distance", DistanceEditor);
            OutputVector2 = LinqSTGNodeOutputViewModel.Vector2("Vector2");

            AddInput("rotation", InputRotation);
            AddInput("distance", InputDistance);
            AddOutput("vector2", OutputVector2);
            AddEditor("rotation", RotationEditor);
            AddEditor("distance", DistanceEditor);

            Name = "Vector2";
            TitleColor = NodeColors.Data;

            OutputVector2.Value = InputRotation.ValueChanged
                .CombineLatest(InputDistance.ValueChanged, (rotation, distance) =>
                    Contextual.Create(dict =>
                    {
                        var rotationValue = rotation?.Invoke(dict) ?? 0;
                        var distanceValue = distance?.Invoke(dict) ?? 0;

                        return new Vector2(
                            DegreeMaths.Cos(rotationValue) * distanceValue,
                            DegreeMaths.Sin(rotationValue) * distanceValue);
                    }));
        }
    }
}
