using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementOperator
{
    /// <summary>
    /// MovementCartesianToPolarNode 的逆变换。
    /// 把上游运动预测点当作笛卡尔点 (x,y)，转成极坐标 (半径r, 角度θ度)，再加 center。
    /// 输出 center + (√(x²+y²), atan2(y,x)·180/π)。θ 为度（与 LuaSTG 运行时及
    /// <see cref="MovementCartesianToPolarNode"/> 的度制约定一致）。
    /// </summary>
    [NodeCreationMenu("Movement/Operator", TitleKey = "linqstg_window_node_movementPolarToCartesian", Order = 10)]
    public class MovementPolarToCartesianNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<IParametric<float, Vector2>>?> InputMovement { get; }
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputCenter { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<float, Vector2>>> OutputMovement { get; }

        public MovementPolarToCartesianNode()
        {
            InputMovement = LinqSTGNodeInputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);
            InputCenter = LinqSTGNodeInputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_center);
            OutputMovement = LinqSTGNodeOutputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);

            AddInput("movement", InputMovement);
            AddInput("center", InputCenter);
            AddOutput("movement", OutputMovement);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_movementPolarToCartesian;
            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputMovement.ValueChanged
                .CombineLatest(InputCenter.ValueChanged, (movement, center)
                    => Contextual.Create(dict
                        => new Parametric<float, Vector2>(t =>
                        {
                            var p = movement?.Invoke(dict)?.Predict(t) ?? Vector2.Zero;
                            var c = center?.Invoke(dict) ?? Vector2.Zero;
                            var q = p - c;
                            var r = MathF.Sqrt(q.X * q.X + q.Y * q.Y);
                            var theta = MathF.Atan2(q.Y, q.X) * 180f / MathF.PI;
                            return c + new Vector2(r, theta);
                        })));
        }
    }
}
