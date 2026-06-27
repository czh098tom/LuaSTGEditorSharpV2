using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementOperator
{
    public class MovementOffsetNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<IParametric<int, Vector2>>?> InputMovement { get; }
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputOffset { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<int, Vector2>>> OutputMovement { get; }

        public MovementOffsetNode()
        {
            InputMovement = LinqSTGNodeInputViewModel.Movement("Movement");
            InputOffset = LinqSTGNodeInputViewModel.Vector2("Offset");
            OutputMovement = LinqSTGNodeOutputViewModel.Movement("Movement");

            AddInput("movement", InputMovement);
            AddInput("offset", InputOffset);
            AddOutput("movement", OutputMovement);

            Name = "Movement Offset";

            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputMovement.ValueChanged
                .CombineLatest(InputOffset.ValueChanged, (movement, offset)
                    => Contextual.Create(dict
                        => new Parametric<int, Vector2>(t
                            => (movement?.Invoke(dict)?.Predict(t) ?? Vector2.Zero)
                                + (offset?.Invoke(dict) ?? Vector2.Zero))));
        }
    }
}
