using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public class ContextFormatter : IFormattable
    {
        public static readonly ContextFormatter Instance = new();

        public string ApplyFormat(string toAppend, params object?[] source)
        {
            return string.Format(toAppend, MergeParam(source));
        }

        private object?[] MergeParam(params object?[] source)
        {
            object?[] fullParams = new object[source.Length + 1];
            fullParams[0] = this;
            Array.Copy(source, 0, fullParams, 1, source.Length);
            return fullParams;
        }

        public override string? ToString()
        {
            return string.Empty;
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if (string.IsNullOrEmpty(format)) return string.Empty;
            formatProvider ??= CultureInfo.CurrentCulture;
            return GetToken(format, formatProvider);
        }

        protected virtual string GetToken(string? format, IFormatProvider formatProvider) => string.Empty;
    }
}
