using System;
using System.Globalization;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel
{
    internal static class NumericConversion
    {
        public static int ToInt32(float value)
        {
            if (!float.IsFinite(value))
            {
                throw new OverflowException($"Cannot convert {value.ToString(CultureInfo.InvariantCulture)} to Int32: the value must be finite.");
            }

            // Widen before comparing: float cannot represent Int32.MaxValue exactly.
            var rounded = Math.Round((double)value, MidpointRounding.ToEven);
            if (rounded < int.MinValue || rounded > int.MaxValue)
            {
                throw new OverflowException($"Cannot convert {value.ToString(CultureInfo.InvariantCulture)} to Int32: the rounded value is out of range.");
            }
            return (int)rounded;
        }
    }
}
