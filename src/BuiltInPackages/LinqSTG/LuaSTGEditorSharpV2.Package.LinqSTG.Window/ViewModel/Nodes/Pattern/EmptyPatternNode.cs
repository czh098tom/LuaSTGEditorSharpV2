using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Pattern
{
    using Pattern = global::LinqSTG.Pattern;

    public class EmptyPatternNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public EmptyPatternNode()
        {
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_pattern);

            AddOutput("pattern", OutputPattern);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_emptyPattern;

            TitleColor = NodeColors.Pattern;

            OutputPattern.Value = Observable.Return(
                Contextual.Create(_ => Pattern.Empty<Parameter, int>()));
        }
    }
}
