using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementOperator
{
    public class MovementAfterTimeNode : LinqSTGNodeViewModel
    {
        public IntegerValueEditorViewModel InputSwitchTimeEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<IParametric<int, Vector2>>?> InputMovement { get; }
        public LinqSTGNodeInputViewModel<Contextual<int>?> InputSwitchTime { get; }
        public LinqSTGNodeInputViewModel<Contextual<IParametric<int, Vector2>>?> InputAfter { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<int, Vector2>>> OutputMovement { get; }

        public MovementAfterTimeNode()
        {
            InputMovement = LinqSTGNodeInputViewModel.Movement("Movement");
            InputSwitchTime = LinqSTGNodeInputViewModel.Int("Switch Time", InputSwitchTimeEditor);
            InputAfter = LinqSTGNodeInputViewModel.Movement("After");
            OutputMovement = LinqSTGNodeOutputViewModel.Movement("Movement");

            AddInput("movement", InputMovement);
            AddInput("switch_time", InputSwitchTime);
            AddInput("after", InputAfter);
            AddOutput("movement", OutputMovement);
            AddEditor("switch_time", InputSwitchTimeEditor);

            Name = "Movement After Time";

            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputMovement.ValueChanged
                .CombineLatest(InputSwitchTime.ValueChanged, InputAfter.ValueChanged,
                    (movement, switchTime, after)
                        => Contextual.Create(dict =>
                        {
                            var source = movement?.Invoke(dict)
                                ?? new Parametric<int, Vector2>(_ => Vector2.Zero);
                            var afterMovement = after?.Invoke(dict)
                                ?? new Parametric<int, Vector2>(_ => Vector2.Zero);
                            var t = switchTime?.Invoke(dict) ?? 0;
                            return new Parametric<int, Vector2>(time =>
                                time < t
                                    ? source.Predict(time)
                                    : afterMovement.Predict(time - t) + source.Predict(t));
                        }));
        }
    }
}
