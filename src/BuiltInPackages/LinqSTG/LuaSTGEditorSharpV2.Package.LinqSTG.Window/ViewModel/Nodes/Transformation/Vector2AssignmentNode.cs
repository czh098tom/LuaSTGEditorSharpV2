using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using System;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Transformation
{
    /// <summary>
    /// 二维赋值节点：<see cref="AssignNode"/> 的二维版本——沿变换链把输入 Vector2
    /// 的两个分量分别写入 key_x/key_y 两个命名变量（可选内联字符串编辑器，默认 x、y），
    /// 即 <c>Floats[key_x] = value.X; Floats[key_y] = value.Y</c>；未连接时取零向量。
    /// </summary>
    [NodeCreationMenu("Assignment", TitleKey = "linqstg_window_node_vector2Assignment", Order = 1)]
    public class Vector2AssignmentNode : LinqSTGNodeViewModel
    {
        public StringValueEditorViewModel KeyXEditor { get; } = new() { RawValue = "x" };
        public StringValueEditorViewModel KeyYEditor { get; } = new() { RawValue = "y" };
        public LinqSTGNodeInputViewModel<Contextual<Parameter>?> InputTransformation { get; }
        public LinqSTGNodeInputViewModel<Contextual<Vector2>?> InputValue { get; }
        public LinqSTGNodeInputViewModel<Contextual<string>?> InputKeyX { get; }
        public LinqSTGNodeInputViewModel<Contextual<string>?> InputKeyY { get; }
        public LinqSTGNodeOutputViewModel<Contextual<Parameter>> OutputTransformation { get; }

        public Vector2AssignmentNode()
        {
            InputTransformation = LinqSTGNodeInputViewModel.Transformation(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_transformation);
            InputValue = LinqSTGNodeInputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_vector2);
            InputKeyX = LinqSTGNodeInputViewModel.String(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_xKey, KeyXEditor);
            InputKeyY = LinqSTGNodeInputViewModel.String(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_yKey, KeyYEditor);
            OutputTransformation = LinqSTGNodeOutputViewModel.Transformation(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_transformation);

            AddInput("transformation", InputTransformation);
            AddInput("value", InputValue);
            AddInput("key_x", InputKeyX);
            AddInput("key_y", InputKeyY);
            AddOutput("transformation", OutputTransformation);
            AddEditor("key_x", KeyXEditor);
            AddEditor("key_y", KeyYEditor);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_vector2Assignment;

            TitleColor = NodeColors.Assignment;

            OutputTransformation.Value = InputTransformation.ValueChanged
                .CombineLatest(InputKeyX.ValueChanged, InputKeyY.ValueChanged, InputValue.ValueChanged,
                    (trans, kx, ky, value) => Contextual.Create(dict =>
                    {
                        dict = trans?.Invoke(dict) ?? dict;

                        var parameter = new Parameter(dict);
                        var keyX = kx?.Invoke(dict) ?? "x";
                        var keyY = ky?.Invoke(dict) ?? "y";
                        var vector = value?.Invoke(dict) ?? Vector2.Zero;

                        parameter.Floats[keyX] = vector.X;
                        parameter.Floats[keyY] = vector.Y;
                        return parameter;
                    }));
        }
    }
}
