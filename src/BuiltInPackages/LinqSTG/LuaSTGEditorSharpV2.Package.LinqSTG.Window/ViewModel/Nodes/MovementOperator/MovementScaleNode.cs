using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementOperator
{
    public class MovementScaleNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<IParametric<int, Vector2>>?> InputMovement { get; }
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputScale { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<int, Vector2>>> OutputMovement { get; }

        public MovementScaleNode()
        {
            InputMovement = LinqSTGNodeInputViewModel.Movement("Movement");
            InputScale = LinqSTGNodeInputViewModel.Vector2("Scale");
            OutputMovement = LinqSTGNodeOutputViewModel.Movement("Movement");

            AddInput("movement", InputMovement);
            AddInput("scale", InputScale);
            AddOutput("movement", OutputMovement);

            Name = "Movement Scale";

            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputMovement.ValueChanged
                .CombineLatest(InputScale.ValueChanged, (movement, scale)
                    => Contextual.Create(dict
                        => new Parametric<int, Vector2>(t =>
                        {
                            var p = movement?.Invoke(dict)?.Predict(t) ?? Vector2.Zero;
                            var s = scale?.Invoke(dict) ?? Vector2.One;
                            return new Vector2(p.X * s.X, p.Y * s.Y);
                        })));
        }
    }
}
