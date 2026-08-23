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
    [NodeCreationMenu("Data", TitleKey = "linqstg_window_node_vector2FromRotationDistance", Order = 5)]
    public class Vector2FromRotationDistanceNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel RotationEditor { get; } = new();
        public FloatValueEditorViewModel DistanceEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputRotation { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputDistance { get; }
        public LinqSTGNodeOutputViewModel<Contextual<Vector2>> OutputVector2 { get; }

        public Vector2FromRotationDistanceNode()
        {
            InputRotation = LinqSTGNodeInputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_rotation, RotationEditor);
            InputDistance = LinqSTGNodeInputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_distance, DistanceEditor);
            OutputVector2 = LinqSTGNodeOutputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_vector2);

            AddInput("rotation", InputRotation);
            AddInput("distance", InputDistance);
            AddOutput("vector2", OutputVector2);
            AddEditor("rotation", RotationEditor);
            AddEditor("distance", DistanceEditor);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_vector2;
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
