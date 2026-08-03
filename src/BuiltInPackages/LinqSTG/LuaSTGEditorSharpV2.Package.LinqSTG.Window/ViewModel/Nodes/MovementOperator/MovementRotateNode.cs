using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementOperator
{
    public class MovementRotateNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel InputAngleEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<IParametric<int, Vector2>>?> InputMovement { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputAngle { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<int, Vector2>>> OutputMovement { get; }

        public MovementRotateNode()
        {
            InputMovement = LinqSTGNodeInputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);
            InputAngle = LinqSTGNodeInputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_angle, InputAngleEditor);
            OutputMovement = LinqSTGNodeOutputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);

            AddInput("movement", InputMovement);
            AddInput("angle", InputAngle);
            AddOutput("movement", OutputMovement);
            AddEditor("angle", InputAngleEditor);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_movementRotate;

            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputMovement.ValueChanged
                .CombineLatest(InputAngle.ValueChanged, (movement, angle)
                    => Contextual.Create(dict
                        => new Parametric<int, Vector2>(t =>
                        {
                            var p = movement?.Invoke(dict)?.Predict(t) ?? Vector2.Zero;
                            var rad = (angle?.Invoke(dict) ?? 0f) * MathF.PI / 180f;
                            var c = MathF.Cos(rad);
                            var s = MathF.Sin(rad);
                            return new Vector2(p.X * c - p.Y * s, p.X * s + p.Y * c);
                        })));
        }
    }
}
