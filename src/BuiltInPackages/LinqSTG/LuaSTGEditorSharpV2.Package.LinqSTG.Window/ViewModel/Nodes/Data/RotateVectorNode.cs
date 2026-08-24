using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data
{
    /// <summary>
    /// 分量原语：把输入 Vector2 绕原点旋转一个角度制的角度，输出旋转后的 Vector2。
    /// 旋转方向与 <see cref="MovementOperator.MovementRotateNode"/> 一致（正角度在数学坐标系下逆时针）；
    /// 预览上屏前会对 Y 取负（见 MainViewModel.UpdatePrediction），故界面上正角度即为逆时针。
    /// 未连接的向量输入按零向量处理，与代码生成的零向量降级保持一致。
    /// </summary>
    [NodeCreationMenu("Data", TitleKey = "linqstg_window_node_rotateVector", Order = 7)]
    public class RotateVectorNode : LinqSTGNodeViewModel
    {
        public FloatValueEditorViewModel AngleEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputVector2 { get; }
        public LinqSTGNodeInputViewModel<Contextual<float>?> InputAngle { get; }
        public LinqSTGNodeOutputViewModel<Contextual<Vector2>> OutputVector2 { get; }

        public RotateVectorNode()
        {
            InputVector2 = LinqSTGNodeInputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_vector2);
            InputAngle = LinqSTGNodeInputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_angle, AngleEditor);
            OutputVector2 = LinqSTGNodeOutputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_vector2);

            AddInput("vector2", InputVector2);
            AddInput("angle", InputAngle);
            AddOutput("vector2", OutputVector2);
            AddEditor("angle", AngleEditor);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_rotateVector;
            TitleColor = NodeColors.Data;

            OutputVector2.Value = InputVector2.ValueChanged
                .CombineLatest(InputAngle.ValueChanged, (vector2, angle)
                    => Contextual.Create(dict =>
                    {
                        var v = vector2?.Invoke(dict) ?? Vector2.Zero;
                        var c = DegreeMaths.Cos(angle?.Invoke(dict) ?? 0f);
                        var s = DegreeMaths.Sin(angle?.Invoke(dict) ?? 0f);
                        return new Vector2(v.X * c - v.Y * s, v.X * s + v.Y * c);
                    }));
        }
    }
}
