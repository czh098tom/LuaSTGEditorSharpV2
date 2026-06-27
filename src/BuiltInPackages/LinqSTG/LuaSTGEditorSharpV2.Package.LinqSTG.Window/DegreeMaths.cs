using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows
{
    public static class DegreeMaths
    {
        public static float Sin(float degree)
        {
            return Convert.ToSingle(Math.Sin(degree * Math.PI / 180));
        }

        public static float Cos(float degree)
        {
            return Convert.ToSingle(Math.Cos(degree * Math.PI / 180));
        }

        public static float Atan2(float y, float x)
        {
            return Convert.ToSingle(Math.Atan2(y, x) * 180 / Math.PI);
        }

        public static float Hypot(float x, float y)
        {
            return Convert.ToSingle(Math.Sqrt(x * x + y * y));
        }
    }
}
