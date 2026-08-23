using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Movement
{
    [NodeCreationMenu("Movement", TitleKey = "linqstg_window_node_uniformAccelerationMovement")]
    public class UniformAccelerationMovementNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputInitialVelocity { get; }
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputAcceleration { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<float, Vector2>>> OutputMovement { get; }

        public UniformAccelerationMovementNode()
        {
            InputInitialVelocity = LinqSTGNodeInputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_initialVelocity);
            InputAcceleration = LinqSTGNodeInputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_acceleration);
            OutputMovement = LinqSTGNodeOutputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);

            AddInput("initial_velocity", InputInitialVelocity);
            AddInput("acceleration", InputAcceleration);
            AddOutput("movement", OutputMovement);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_uniformAccelerationMovement;

            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputInitialVelocity.ValueChanged
                .CombineLatest(InputAcceleration.ValueChanged, (velocity, acceleration)
                    => Contextual.Create(dict
                        => new Parametric<float, Vector2>(t =>
                        {
                            var v = velocity?.Invoke(dict) ?? Vector2.Zero;
                            var a = acceleration?.Invoke(dict) ?? Vector2.Zero;
                            return v * t + a * t * t / 2f;
                        })));
        }
    }
}
