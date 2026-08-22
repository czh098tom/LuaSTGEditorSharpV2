using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using global::LinqSTG.Kinematics;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Movement
{
    /// <summary>
    /// 点→运动的桥接节点：把一个点（Vector2 子图）包装成运动，
    /// 点子图按采样时间 t 惰性求值（每个 t 注入 <see cref="TransformContext{Time=t}"/>）。
    /// 是统一自定义变换路径 Movement-Predict-FromPointMovement 的收口半段：
    /// 子图内 Predict 采样输入运动、经点级运算后由本节点包装回运动，
    /// 直接作为运动链的一环使用（无需独立的变换收口节点）。
    /// 点子图不含 InputTime 时退化为常量运动（原名 StationaryMovement 的语义）。
    /// </summary>
    public class FromPointMovementNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputPosition { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IParametric<float, Vector2>>> OutputMovement { get; }

        public FromPointMovementNode()
        {
            InputPosition = LinqSTGNodeInputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_position);
            OutputMovement = LinqSTGNodeOutputViewModel.Movement(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_movement);

            AddInput("position", InputPosition);
            AddOutput("movement", OutputMovement);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_fromPointMovement;
            TitleColor = NodeColors.Movement;

            OutputMovement.Value = InputPosition.ValueChanged
                .Select(vec
                    => Contextual.Create(dict
                        => new Parametric<float, Vector2>(t
                            => vec?.Invoke(dict.WithTransform(new TransformContext { Time = t }))
                                ?? Vector2.Zero)));
        }
    }
}
