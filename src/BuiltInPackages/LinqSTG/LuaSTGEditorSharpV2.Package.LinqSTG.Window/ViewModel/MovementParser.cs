using global::LinqSTG.Kinematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    public delegate IParametric<TTime, TData> MovementParser<TTime, TData>(Dictionary<string, float> param);
}
