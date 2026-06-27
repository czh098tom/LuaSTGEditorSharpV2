using NodeNetwork.Toolkit.ValueNode;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.PatternOperator
{
    public class TrimPatternNode : LinqSTGNodeViewModel
    {
        public LinqSTGNodeInputViewModel<Contextual<IPattern<Parameter, int>>?> InputPattern { get; }
        public LinqSTGNodeOutputViewModel<Contextual<IPattern<Parameter, int>>> OutputPattern { get; }

        public TrimPatternNode()
        {
            InputPattern = LinqSTGNodeInputViewModel.Pattern("Pattern");
            OutputPattern = LinqSTGNodeOutputViewModel.Pattern("Pattern");

            AddInput("pattern", InputPattern);
            AddOutput("pattern", OutputPattern);

            Name = "Trim";

            TitleColor = NodeColors.PatternOperator;

            OutputPattern.Value = InputPattern.ValueChanged
                .Select(pattern => Contextual.Create(dict =>
                    pattern?.Invoke(dict)?.Trim() ?? global::LinqSTG.Pattern.Empty<Parameter, int>()));
        }
    }
}
