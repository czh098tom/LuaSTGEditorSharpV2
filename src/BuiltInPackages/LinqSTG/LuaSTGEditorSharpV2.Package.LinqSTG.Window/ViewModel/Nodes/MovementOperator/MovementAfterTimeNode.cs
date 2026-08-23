using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementOperator
{
    [NodeCreationMenu("MovementOperator", TitleKey = "linqstg_window_node_movementAfterTime")]
    public class MovementAfterTimeNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel InputSwitchTimeEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<IParametric<float, Vector2>>?> InputMovement { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputSwitchTime { get; }
        public LinqSTGNodeInputViewModel<Contextual<IParametric<float, Vector2>>?> InputAfter { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<float, Vector2>>> OutputMovement { get; }

        public MovementAfterTimeNode()
        {
            InputMovement = LinqSTGNodeInputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);
            InputSwitchTime = LinqSTGNodeInputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_switchTime, InputSwitchTimeEditor);
            InputAfter = LinqSTGNodeInputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_after);
            OutputMovement = LinqSTGNodeOutputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);

            AddInput("movement", InputMovement);
            AddInput("switch_time", InputSwitchTime);
            AddInput("after", InputAfter);
            AddOutput("movement", OutputMovement);
            AddEditor("switch_time", InputSwitchTimeEditor);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_movementAfterTime;

            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputMovement.ValueChanged
                .CombineLatest(InputSwitchTime.ValueChanged, InputAfter.ValueChanged,
                    (movement, switchTime, after)
                        => Contextual.Create(dict =>
                        {
                            var source = movement?.Invoke(dict)
                                ?? new Parametric<float, Vector2>(_ => Vector2.Zero);
                            var afterMovement = after?.Invoke(dict)
                                ?? new Parametric<float, Vector2>(_ => Vector2.Zero);
                            var t = switchTime?.Invoke(dict) ?? 0f;
                            return new Parametric<float, Vector2>(time =>
                                time < t
                                    ? source.Predict(time)
                                    : afterMovement.Predict(time - t) + source.Predict(t));
                        }));
        }
    }
}
