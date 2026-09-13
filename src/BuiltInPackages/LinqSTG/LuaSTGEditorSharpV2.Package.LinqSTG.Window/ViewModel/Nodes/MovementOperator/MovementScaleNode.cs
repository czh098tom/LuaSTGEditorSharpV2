using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementOperator
{
    [NodeCreationMenu("Movement/Operator", TitleKey = "linqstg_window_node_movementScale", Order = 8)]
    public class MovementScaleNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<IParametric<float, Vector2>>?> InputMovement { get; }
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputScale { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<float, Vector2>>> OutputMovement { get; }

        public MovementScaleNode()
        {
            InputMovement = LinqSTGNodeInputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);
            InputScale = LinqSTGNodeInputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_scale);
            OutputMovement = LinqSTGNodeOutputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);

            AddInput("movement", InputMovement);
            AddInput("scale", InputScale);
            AddOutput("movement", OutputMovement);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_movementScale;

            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputMovement.ValueChanged
                .CombineLatest(InputScale.ValueChanged, (movement, scale)
                    => Contextual.Create(dict
                        => new Parametric<float, Vector2>(t =>
                        {
                            var p = movement?.Invoke(dict)?.Predict(t) ?? Vector2.Zero;
                            var s = scale?.Invoke(dict) ?? Vector2.One;
                            return new Vector2(p.X * s.X, p.Y * s.Y);
                        })));
        }
    }
}
