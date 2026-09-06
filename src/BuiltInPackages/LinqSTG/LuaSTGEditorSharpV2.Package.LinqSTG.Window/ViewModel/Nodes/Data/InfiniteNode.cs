using System;
using System.Reactive.Linq;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Data
{
    /// <summary>
    /// The built-in <c>_infinite</c> variable: the loop bound consumed by the
    /// generated Shoot loop (integer 10). The preview reads the value from the
    /// variable list seeded into the root parameter; translation emits the
    /// outer-scope Lua variable <c>_infinite</c> verbatim.
    /// Created by dragging the locked list entry into the blueprint area.
    /// </summary>
    [NodeCreationMenu("Data", TitleKey = "linqstg_window_node_infinite", Order = 7)]
    public class InfiniteNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeOutputViewModel<Contextual<int>> OutputValue { get; }

        public InfiniteNode()
        {
            OutputValue = LinqSTGNodeOutputViewModel.Int(
                global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_value);

            AddOutput("value", OutputValue);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_infinite;

            TitleColor = NodeColors.Data;

            OutputValue.Value = Observable.Return(Contextual.Create<int>(dict =>
                Convert.ToInt32(dict.Floats.GetValueOrDefault(VariableListViewModel.InfiniteName, (float)VariableListViewModel.InfiniteDefaultValue))));
        }
    }
}
