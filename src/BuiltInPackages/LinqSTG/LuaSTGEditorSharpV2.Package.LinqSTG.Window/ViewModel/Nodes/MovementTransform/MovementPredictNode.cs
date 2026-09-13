using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementTransformOperator
{
    /// <summary>
    /// 运动采样节点：在指定时间 t' 处对运动求值，输出点（Vector2）。
    /// <c>OutputPoint = movement.Predict(time)</c>。
    /// 是统一自定义变换路径 Movement-Predict-FromPointMovement 的入口半段：
    /// 运动由 movement 端口显式连入，配合 <see cref="MovementTransformInputTimeNode"/>
    /// （在 <see cref="Nodes.Movement.FromPointMovementNode"/> 的采样作用域内读当前 t）
    /// 与 Math 节点构造 φ(t)，可表达 <c>f(t) ↦ f(φ(t))</c>。
    /// 端口未连接时安全降级：零运动采样、t' 取 0。
    /// </summary>
    [NodeCreationMenu("Movement/Operator", TitleKey = "linqstg_window_node_movementPredict", Order = 6)]
    public class MovementPredictNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<IParametric<float, Vector2>>?> InputMovement { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputTime { get; }
        public LinqSTGNodeOutputViewModel<Contextual<Vector2>> OutputPoint { get; }

        public MovementPredictNode()
        {
            InputMovement = LinqSTGNodeInputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);
            InputTime = LinqSTGNodeInputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_time);
            OutputPoint = LinqSTGNodeOutputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_point);

            AddInput("movement", InputMovement);
            AddInput("time", InputTime);
            AddOutput("point", OutputPoint);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_movementPredict;
            TitleColor = NodeColors.Movement;

            OutputPoint.Value = InputMovement.ValueChanged
                .CombineLatest(InputTime.ValueChanged, (movement, time)
                    => Contextual.Create(dict =>
                    {
                        var m = movement?.Invoke(dict)
                            ?? new Parametric<float, Vector2>(_ => Vector2.Zero);
                        var t = time?.Invoke(dict) ?? 0f;
                        return m.Predict(t);
                    }));
        }
    }
}
