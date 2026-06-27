using System;
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
    }
}
