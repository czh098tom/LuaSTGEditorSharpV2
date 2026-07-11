using System.Drawing;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows
{
    public readonly record struct BulletVisual(PointF Position, BulletShape Shape, float Diameter);
}
