using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.MovementTransformOperator
{
    /// <summary>
    /// 分量原语：输出当前正在被变换的点 p（Vector2）。
    /// 仅在 MovementMapNode.Transform 上游子图中有意义：收口节点求值时通过
    /// TransformScope 把当前 p 注入环境，本节点读 dict.Transform.Point。
    /// 离开变换求值时返回 Vector2.Zero（安全降级）。
    /// 配合 Vector2SplitNode 与现有 Math 节点，可拼出依赖 p 分量的任意变换。
    /// </summary>
    public class MovementTransformInputPointNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeOutputViewModel<Contextual<Vector2>> OutputPoint { get; }

        public MovementTransformInputPointNode()
        {
            OutputPoint = LinqSTGNodeOutputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_point);

            AddOutput("point", OutputPoint);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_transformInputPoint;
            TitleColor = NodeColors.TransformInput;

            OutputPoint.Value = Observable.Return(
                Contextual.Create(dict => dict.Transform?.Point ?? Vector2.Zero));
        }
    }
}
