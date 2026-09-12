using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data
{
    /// <summary>
    /// 变量节点：key 输入（可选内联字符串编辑器）同时引出两个输出——
    /// "key" 原样输出变量名（string），"value" 按 <see cref="IntrinsicOperator.TakeVariableFromContextNode"/>
    /// 的逻辑从参数层取 float 值（缺失时为 0）。
    /// </summary>
    [NodeCreationMenu("Data", TitleKey = "linqstg_window_node_variable", Order = 6.5)]
    public class VariableNode : LinqSTGNodeViewModel
    {
        public StringValueEditorViewModel KeyEditor { get; } = new();
        public LinqSTGNodeInputViewModel<Contextual<string>?> InputKey { get; }
        public LinqSTGNodeOutputViewModel<Contextual<string>> OutputKey { get; }
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public VariableNode()
        {
            InputKey = LinqSTGNodeInputViewModel.String(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_key, KeyEditor);
            OutputKey = LinqSTGNodeOutputViewModel.String(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_key);
            OutputValue = LinqSTGNodeOutputViewModel.Float(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_value);

            AddInput("key", InputKey);
            AddOutput("key", OutputKey);
            AddOutput("value", OutputValue);
            AddEditor("key", KeyEditor);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_variable;

            TitleColor = NodeColors.Data;

            OutputKey.Value = InputKey.ValueChanged
                .Select(key => Contextual.Create<string>(dict => key?.Invoke(dict) ?? string.Empty));
            OutputValue.Value = InputKey.ValueChanged
                .Select(key => Contextual.Create<float>(dict =>
                    dict.Floats.GetValueOrDefault(key?.Invoke(dict) ?? string.Empty, 0f)));
        }
    }
}
