using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data
{
    /// <summary>
    /// Base of the variable list reference nodes. The node carries the entry's
    /// name: the preview reads the value from the pattern's parameter scope (the
    /// variable list is seeded into the root parameter), while translation emits
    /// the name verbatim as an outer-scope Lua variable.
    /// Usually created by dragging a list entry into the blueprint area; the
    /// entry's value type decides between the int and float variants.
    /// </summary>
    public abstract class PatternVariableNodeBase : LinqSTGNodeViewModel
    {
        public const string NameEditorKey = "name";

        public StringValueEditorViewModel NameEditor { get; } = new();

        protected PatternVariableNodeBase()
        {
            AddEditor(NameEditorKey, NameEditor);
            TitleColor = NodeColors.Data;
        }
    }

    /// <summary>Variable reference producing a float value; mirrors float-typed list entries.</summary>
    [NodeCreationMenu("Data", TitleKey = "linqstg_window_node_variableFloat", Order = 5)]
    public class PatternVariableFloatNode : PatternVariableNodeBase
    {
        public LinqSTGNodeOutputViewModel<Contextual<float>> OutputValue { get; }

        public PatternVariableFloatNode()
        {
            OutputValue = LinqSTGNodeOutputViewModel.Float(
                global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_value);
            OutputValue.Editor = NameEditor;

            AddOutput("value", OutputValue);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_variableFloat;

            OutputValue.Value = NameEditor.ValueChanged
                .Select(name => Contextual.Create<float>(dict =>
                    dict.Floats.GetValueOrDefault(name?.Invoke(dict) ?? string.Empty, 0f)));
        }
    }

    /// <summary>Variable reference producing an int value; mirrors int-typed list entries.</summary>
    [NodeCreationMenu("Data", TitleKey = "linqstg_window_node_variableInt", Order = 6)]
    public class PatternVariableIntNode : PatternVariableNodeBase
    {
        public LinqSTGNodeOutputViewModel<Contextual<int>> OutputValue { get; }

        public PatternVariableIntNode()
        {
            OutputValue = LinqSTGNodeOutputViewModel.Int(
                global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_value);
            OutputValue.Editor = NameEditor;

            AddOutput("value", OutputValue);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_variableInt;

            OutputValue.Value = NameEditor.ValueChanged
                .Select(name => Contextual.Create<int>(dict =>
                    Convert.ToInt32(dict.Floats.GetValueOrDefault(name?.Invoke(dict) ?? string.Empty, 0f))));
        }
    }
}
