using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data
{
    /// <summary>
    /// 二维变量节点：聚合两个 <see cref="VariableNode"/> 的输入和输出——
    /// key_x/key_y 输入（可选内联字符串编辑器，默认 x、y）各自引出 "key_x"/"key_y"
    /// 原样输出变量名（string）与 "x"/"y" 分量输出（float，缺失时为 0），
    /// 另有 "vector2" 输出把两个变量直接组成 <see cref="Vector2"/> 端口。
    /// </summary>
    [NodeCreationMenu("Data", TitleKey = "linqstg_window_node_vector2Variable", Order = 2)]
    public class Vector2VariableNode : LinqSTGNodeViewModel
    {
        public StringValueEditorViewModel KeyXEditor { get; } = new() { RawValue = "x" };
        public StringValueEditorViewModel KeyYEditor { get; } = new() { RawValue = "y" };
        public LinqSTGNodeInputViewModel<Contextual<string>?> InputKeyX { get; }
        public LinqSTGNodeInputViewModel<Contextual<string>?> InputKeyY { get; }
        public LinqSTGNodeOutputViewModel<Contextual<string>> OutputKeyX { get; }
        public LinqSTGNodeOutputViewModel<Contextual<string>> OutputKeyY { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputX { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputY { get; }
        public LinqSTGNodeOutputViewModel<Contextual<Vector2>> OutputVector2 { get; }

        public Vector2VariableNode()
        {
            InputKeyX = LinqSTGNodeInputViewModel.String(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_xKey, KeyXEditor);
            InputKeyY = LinqSTGNodeInputViewModel.String(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_yKey, KeyYEditor);
            OutputKeyX = LinqSTGNodeOutputViewModel.String(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_xKey);
            OutputKeyY = LinqSTGNodeOutputViewModel.String(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_yKey);
            OutputX = LinqSTGNodeOutputViewModel.Float("X");
            OutputY = LinqSTGNodeOutputViewModel.Float("Y");
            OutputVector2 = LinqSTGNodeOutputViewModel.Vector2(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_vector2);

            AddInput("key_x", InputKeyX);
            AddInput("key_y", InputKeyY);
            AddOutput("key_x", OutputKeyX);
            AddOutput("key_y", OutputKeyY);
            AddOutput("x", OutputX);
            AddOutput("y", OutputY);
            AddOutput("vector2", OutputVector2);
            AddEditor("key_x", KeyXEditor);
            AddEditor("key_y", KeyYEditor);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_vector2Variable;

            TitleColor = NodeColors.Data;

            OutputKeyX.Value = InputKeyX.ValueChanged
                .Select(key => Contextual.Create<string>(dict => key?.Invoke(dict) ?? string.Empty));
            OutputKeyY.Value = InputKeyY.ValueChanged
                .Select(key => Contextual.Create<string>(dict => key?.Invoke(dict) ?? string.Empty));

            OutputVector2.Value = InputKeyX.ValueChanged
                .CombineLatest(InputKeyY.ValueChanged, (kx, ky) => Contextual.Create<Vector2>(dict =>
                    new Vector2(
                        dict.Floats.GetValueOrDefault(kx?.Invoke(dict) ?? string.Empty, 0f),
                        dict.Floats.GetValueOrDefault(ky?.Invoke(dict) ?? string.Empty, 0f))));
            OutputX.Value = OutputVector2.Value.Select(v => Contextual.Create<float>(dict => (v?.Invoke(dict) ?? Vector2.Zero).X));
            OutputY.Value = OutputVector2.Value.Select(v => Contextual.Create<float>(dict => (v?.Invoke(dict) ?? Vector2.Zero).Y));
        }
    }
}
