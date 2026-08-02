using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementOperator
{
    /// <summary>
    /// 把上游运动预测点当作 (角度θ=弧度, 半径r) 解释为极坐标，展开回笛卡尔坐标。
    /// 严格语义：q = p - center；输出 center + (r·cos(θ), r·sin(θ))，
    /// 其中 θ = q.X（弧度），r = q.Y。即用户给定的 (p.y·cos(p.x), p.y·sin(p.x)) 形式。
    /// </summary>
    public class MovementCartesianToPolarNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<IParametric<int, Vector2>>?> InputMovement { get; }
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputCenter { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<int, Vector2>>> OutputMovement { get; }

        public MovementCartesianToPolarNode()
        {
            InputMovement = LinqSTGNodeInputViewModel.Movement("Movement");
            InputCenter = LinqSTGNodeInputViewModel.Vector2("Center");
            OutputMovement = LinqSTGNodeOutputViewModel.Movement("Movement");

            AddInput("movement", InputMovement);
            AddInput("center", InputCenter);
            AddOutput("movement", OutputMovement);

            Name = "Movement Cartesian To Polar";
            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputMovement.ValueChanged
                .CombineLatest(InputCenter.ValueChanged, (movement, center)
                    => Contextual.Create(dict
                        => new Parametric<int, Vector2>(t =>
                        {
                            var p = movement?.Invoke(dict)?.Predict(t) ?? Vector2.Zero;
                            var c = center?.Invoke(dict) ?? Vector2.Zero;
                            var q = p - c;
                            return c + new Vector2(q.Y * MathF.Cos(q.X), q.Y * MathF.Sin(q.X));
                        })));
        }
    }
}
