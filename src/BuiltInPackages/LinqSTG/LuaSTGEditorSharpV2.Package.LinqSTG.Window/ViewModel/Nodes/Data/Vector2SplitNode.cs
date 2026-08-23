using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data
{
    /// <summary>
    /// 分量原语：把输入 Vector2 拆成 X、Y 两个 Float 输出。
    /// 供用户在变换子图里把 InputPoint 的点拆成分量，喂给 Math 节点加工。
    /// 合成方向已有 <see cref="Vector2Node"/>（Float×2 → Vector2）。
    /// </summary>
    [NodeCreationMenu("Data", TitleKey = "linqstg_window_node_vector2Split", Order = 6)]
    public class Vector2SplitNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputVector2 { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputX { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputY { get; }

        public Vector2SplitNode()
        {
            InputVector2 = LinqSTGNodeInputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_vector2);
            OutputX = LinqSTGNodeOutputViewModel.Float("X");
            OutputY = LinqSTGNodeOutputViewModel.Float("Y");

            AddInput("vector2", InputVector2);
            AddOutput("x", OutputX);
            AddOutput("y", OutputY);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_vector2Split;
            TitleColor = NodeColors.Data;

            OutputX.Value = InputVector2.ValueChanged
                .Select(v => Contextual.Create(dict => (v?.Invoke(dict) ?? Vector2.Zero).X));
            OutputY.Value = InputVector2.ValueChanged
                .Select(v => Contextual.Create(dict => (v?.Invoke(dict) ?? Vector2.Zero).Y));
        }
    }
}
