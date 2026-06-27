using global::LinqSTG.Kinematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows
{
    public readonly record struct PointPrediction(IParametric<int, Vector2> PointFunc, int StartTime)
    {
    }
}
