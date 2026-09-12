using System.Linq;
using System.Reactive.Linq;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Editor;
using NodeNetwork.Toolkit.ValueNode;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data
{
    /// <summary>
    /// The built-in <c>_infinite</c> variable: the loop bound consumed by the
    /// generated Shoot loop. Inserted from the right-click menu, not a variable
    /// list entry. Its editor only feeds the preview (each node instance tunes
    /// its own value); translation always emits the outer-scope Lua variable
    /// verbatim.
    /// </summary>
    [NodeCreationMenu("Data", TitleKey = "linqstg_window_node_infinite", Order = 7)]
    public class InfiniteNode : LinqSTGNodeViewModel
    {
        public const string PreviewValueInputKey = "preview_value";

        public IntegerValueEditorViewModel PreviewValueEditor { get; } = new()
        {
            RawValue = (int)VariableListViewModel.InfiniteDefaultValue,
        };

        public LinqSTGNodeInputViewModel<Contextual<int>?> InputPreviewValue { get; }
        public LinqSTGNodeOutputViewModel<Contextual<int>> OutputValue { get; }

        public InfiniteNode()
        {
            InputPreviewValue = new LinqSTGNodeInputViewModel<Contextual<int>?>
            {
                Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_previewValue,
                Editor = PreviewValueEditor,
                Port = null
            };
            OutputValue = LinqSTGNodeOutputViewModel.Int(
                global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_value);

            AddInput(PreviewValueInputKey, InputPreviewValue);
            AddOutput("value", OutputValue);
            AddEditor(PreviewValueInputKey, PreviewValueEditor);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_infinite;

            TitleColor = NodeColors.Data;

            OutputValue.Value = InputPreviewValue.ValueChanged
                .Select(preview => Contextual.Create<int>(dict =>
                    preview?.Invoke(dict) ?? (int)VariableListViewModel.InfiniteDefaultValue));
        }
    }
}
