using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator
{
    public static class StringBuilderExtension
    {
        public static StringBuilder AppendIndented(this StringBuilder builder, StringBuilder indention, string toAppend)
        {
            string[] seg = toAppend.Split('\n');
            for (int i = 0; i < seg.Length; i++)
            {
                if (seg[i] != string.Empty)
                {
                    builder.Append(indention);
                    builder.Append(seg[i]);
                    builder.Append('\n');
                }
            }
            return builder;
        }

        public static StringBuilder AppendIndentedFormat(this StringBuilder builder, StringBuilder indention
            , string toAppend, params object?[] source)
        {
            return builder.AppendIndented(indention, string.Format(toAppend, source));
        }
    }
}
