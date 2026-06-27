using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Movement
{
    public class UniformAccelerationMovementNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputInitialVelocity { get; }
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputAcceleration { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<int, Vector2>>> OutputMovement { get; }

        public UniformAccelerationMovementNode()
        {
            InputInitialVelocity = LinqSTGNodeInputViewModel.Vector2("Initial Velocity");
            InputAcceleration = LinqSTGNodeInputViewModel.Vector2("Acceleration");
            OutputMovement = LinqSTGNodeOutputViewModel.Movement("Movement");

            AddInput("initial_velocity", InputInitialVelocity);
            AddInput("acceleration", InputAcceleration);
            AddOutput("movement", OutputMovement);

            Name = "Uniform Acceleration Movement";

            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputInitialVelocity.ValueChanged
                .CombineLatest(InputAcceleration.ValueChanged, (velocity, acceleration)
                    => Contextual.Create(dict
                        => new Parametric<int, Vector2>(t =>
                        {
                            var v = velocity?.Invoke(dict) ?? Vector2.Zero;
                            var a = acceleration?.Invoke(dict) ?? Vector2.Zero;
                            return v * t + a * t * t / 2f;
                        })));
        }
    }
}
