using global::LinqSTG.Kinematics;
using System.Numerics;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows
{
    public readonly record struct PointPrediction(
        IParametric<int, Vector2> PointFunc,
        int StartTime,
        BulletShape Shape,
        float Diameter)
    {
    }
}
