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
        public LinqSTGNodeInputViewModel<Contextual<IParametric<float, Vector2>>?> InputMovement { get; }
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputOffset { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<float, Vector2>>> OutputMovement { get; }

        public MovementOffsetNode()
        {
            InputMovement = LinqSTGNodeInputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);
            InputOffset = LinqSTGNodeInputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_offset);
            OutputMovement = LinqSTGNodeOutputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);

            AddInput("movement", InputMovement);
            AddInput("offset", InputOffset);
            AddOutput("movement", OutputMovement);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_movementOffset;

            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputMovement.ValueChanged
                .CombineLatest(InputOffset.ValueChanged, (movement, offset)
                    => Contextual.Create(dict
                        => new Parametric<float, Vector2>(t
                            => (movement?.Invoke(dict)?.Predict(t) ?? Vector2.Zero)
                                + (offset?.Invoke(dict) ?? Vector2.Zero))));
        }
    }
}
