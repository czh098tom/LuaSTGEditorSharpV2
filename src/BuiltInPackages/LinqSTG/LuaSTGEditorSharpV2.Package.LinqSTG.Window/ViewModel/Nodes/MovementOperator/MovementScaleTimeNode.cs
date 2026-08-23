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
    /// 时间缩放节点：<c>f(t) ↦ f(a·t)</c>。新运动在时间 t 处的位置 = 原运动在 a·t 处的位置。
    /// 用于变速回放（a&gt;1 加速、0&lt;a&lt;1 减速、a&lt;0 反向）。
    /// 属于时间重映射原语：新运动的 t 自包含在 Parametric 闭包里，不读 TransformContext。
    /// </summary>
    [NodeCreationMenu("MovementOperator", TitleKey = "linqstg_window_node_movementScaleTime")]
    public class MovementScaleTimeNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel InputFactorEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<IParametric<float, Vector2>>?> InputMovement { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputFactor { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<float, Vector2>>> OutputMovement { get; }

        public MovementScaleTimeNode()
        {
            InputMovement = LinqSTGNodeInputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);
            InputFactor = LinqSTGNodeInputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_factor, InputFactorEditor);
            OutputMovement = LinqSTGNodeOutputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);

            AddInput("movement", InputMovement);
            AddInput("factor", InputFactor);
            AddOutput("movement", OutputMovement);
            AddEditor("factor", InputFactorEditor);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_movementScaleTime;

            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputMovement.ValueChanged
                .CombineLatest(InputFactor.ValueChanged, (movement, factor)
                    => Contextual.Create(dict
                        => new Parametric<float, Vector2>(t =>
                        {
                            var m = movement?.Invoke(dict) ?? new Parametric<float, Vector2>(_ => Vector2.Zero);
                            var a = factor?.Invoke(dict) ?? 1f;
                            return m.Predict(a * t);
                        })));
        }
    }
}
