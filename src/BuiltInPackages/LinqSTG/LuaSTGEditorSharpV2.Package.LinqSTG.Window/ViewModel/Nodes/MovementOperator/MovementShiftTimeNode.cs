using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementOperator
{
    /// <summary>
    /// 时间平移节点：<c>f(t) ↦ f(t + Δ)</c>。新运动在时间 t 处的位置 = 原运动在 t+Δ 处的位置。
    /// 用于相位偏移（Δ&gt;0 前瞻、Δ&lt;0 滞后）。
    /// 属于时间重映射原语：新运动的 t 自包含在 Parametric 闭包里，不读 TransformContext。
    /// </summary>
    [NodeCreationMenu("MovementOperator", TitleKey = "linqstg_window_node_movementShiftTime")]
    public class MovementShiftTimeNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel InputDeltaEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<IParametric<float, Vector2>>?> InputMovement { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputDelta { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<float, Vector2>>> OutputMovement { get; }

        public MovementShiftTimeNode()
        {
            InputMovement = LinqSTGNodeInputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);
            InputDelta = LinqSTGNodeInputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_delta, InputDeltaEditor);
            OutputMovement = LinqSTGNodeOutputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);

            AddInput("movement", InputMovement);
            AddInput("delta", InputDelta);
            AddOutput("movement", OutputMovement);
            AddEditor("delta", InputDeltaEditor);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_movementShiftTime;

            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputMovement.ValueChanged
                .CombineLatest(InputDelta.ValueChanged, (movement, delta)
                    => Contextual.Create(dict
                        => new Parametric<float, Vector2>(t =>
                        {
                            var m = movement?.Invoke(dict) ?? new Parametric<float, Vector2>(_ => Vector2.Zero);
                            var d = delta?.Invoke(dict) ?? 0f;
                            return m.Predict(t + d);
                        })));
        }
    }
}
