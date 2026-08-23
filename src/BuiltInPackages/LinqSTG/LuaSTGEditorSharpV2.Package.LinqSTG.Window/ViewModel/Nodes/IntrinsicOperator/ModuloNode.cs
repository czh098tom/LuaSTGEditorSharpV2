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
    [NodeCreationMenu("Operator", EnglishTitle = "Modulo (%)", Order = 4)]
    public class ModuloNode : LinqSTGNodeViewModel
    {
        public ContextAwareNodeInputViewModel NumericA { get; }
        public ContextAwareNodeInputViewModel NumericB { get; }
        public ContextAwareNodeOutputViewModel NumericResult { get; }

        public ModuloNode()
        {
            NumericA = LinqSTGNodeInputViewModel.Numeric();
            NumericB = LinqSTGNodeInputViewModel.Numeric();
            NumericResult = LinqSTGNodeOutputViewModel.Numeric();

            AddInput("a", NumericA);
            AddInput("b", NumericB);
            AddOutput("result", NumericResult);

            Name = "%";
            TitleColor = NodeColors.Operator;

            NumericResult.Type = NumericA.TypeChanged
                .CombineLatest(NumericB.TypeChanged,
                    (type1, type2) => type1 == type2 ? type1 : null);

            NumericResult.Value = NumericA.ValueChanged.CombineLatest(NumericB.ValueChanged, TryModulo);
        }

        private static object? TryModulo(object? lhs, object? rhs)
        {
            if (lhs is Contextual<int> cint1 && rhs is Contextual<int> cint2)
            {
                return Contextual.Create(dict => cint1(dict) % cint2(dict));
            }
            if (lhs is Contextual<float> cfloat1 && rhs is Contextual<float> cfloat2)
            {
                return Contextual.Create(dict => cfloat1(dict) % cfloat2(dict));
            }
            // Modulo is not meaningful on Vector2 in Lua; only int/float are supported.
            return null;
        }
    }
}
