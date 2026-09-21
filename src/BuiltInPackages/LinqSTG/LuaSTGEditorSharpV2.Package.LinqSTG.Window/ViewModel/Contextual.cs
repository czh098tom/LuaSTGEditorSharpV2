using System;
using NodeNetwork.ViewModels;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    public delegate T Contextual<out T>(Parameter param);

    public static class Contextual
    {
        public static Contextual<T> Create<T>(Func<Parameter, T> func)
        {
            return new Contextual<T>(func);
        }

        public static Contextual<T> Create<T>(Func<Parameter, T> func, NodeOutputViewModel output)
        {
            return parameter =>
            {
                try
                {
                    return func(parameter);
                }
                catch (ArithmeticException exception)
                {
                    if (output.Port is LinqSTGPortViewModel port)
                    {
                        port.EvaluationError = exception.Message;
                    }
                    throw new ContextualEvaluationException(exception.Message, exception);
                }
            };
        }
    }
}
