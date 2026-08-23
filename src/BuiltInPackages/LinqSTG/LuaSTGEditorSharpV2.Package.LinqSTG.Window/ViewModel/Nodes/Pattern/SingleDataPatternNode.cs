using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.Pattern
{
    using Pattern = global::LinqSTG.Pattern;

    [NodeCreationMenu("Pattern", TitleKey = "linqstg_window_node_singlePattern")]
    public class SingleDataPatternNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<Parameter>?> InputTransformation { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public SingleDataPatternNode()
        {
            InputTransformation = LinqSTGNodeInputViewModel.Transformation(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_transformation);
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern(global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_port_pattern);

            AddInput("transformation", InputTransformation);
            AddOutput("pattern", OutputPattern);

            Name = global::LuaSTGEditorSharpV2.Package.LinqSTG.Windows.Resources.Localized.linqstg_window_node_singlePattern;

            TitleColor = NodeColors.Pattern;

            OutputPattern.Value = InputTransformation.ValueChanged
                .Select(trans => Contextual.Create(dict =>
                    Pattern.Single<Parameter, int>(trans?.Invoke(dict) ?? new Parameter(dict))));
        }
    }
}
