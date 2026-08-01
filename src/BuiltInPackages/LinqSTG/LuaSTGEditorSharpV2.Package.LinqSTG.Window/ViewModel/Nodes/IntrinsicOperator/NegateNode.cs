using global::LinqSTG.Kinematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes.IntrinsicOperator
{
    public class NegateNode : LinqSTGNodeViewModel
    {
        public ContextAwareNodeInputViewModel NumericA { get; }
        public ContextAwareNodeOutputViewModel NumericResult { get; }

        public NegateNode()
        {
            NumericA = LinqSTGNodeInputViewModel.Numeric();
            NumericResult = LinqSTGNodeOutputViewModel.Numeric();

            AddInput("a", NumericA);
            AddOutput("result", NumericResult);

            Name = "neg";
            TitleColor = NodeColors.Operator;

            NumericResult.Type = NumericA.TypeChanged;

            NumericResult.Value = NumericA.ValueChanged.Select(TryNegate);
        }

        private static object? TryNegate(object? x)
        {
            if (x is Contextual<int> cint)
            {
                return Contextual.Create(dict => -cint(dict));
            }
            if (x is Contextual<float> cfloat)
            {
                return Contextual.Create(dict => -cfloat(dict));
            }
            if (x is Contextual<Vector2> cvec)
            {
                return Contextual.Create(dict =>
                    Parametric.Create<int, Vector2>(t => -cvec(dict)));
            }
            return null;
        }
    }
}
