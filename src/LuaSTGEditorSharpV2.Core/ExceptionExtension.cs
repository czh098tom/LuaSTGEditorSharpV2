using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public static class ExceptionExtensions
    {
        static string FormatException(System.Exception e)
            => new StringBuilder()
                .AppendLine("Error:" + e.GetType().FullName)
                .AppendLine("Message:" + e.Message)
                .AppendLine("Stack Trace:")
                .Append(e.StackTrace).ToString();

        public static string ErrorLogBuilder(System.Exception e, bool showInnerException)
        {
            var sb = new StringBuilder().AppendLine()
                .AppendLine("==============ERROR==============")
                .AppendLine(FormatException(e));
            if (showInnerException)
            {
                while (e.InnerException != null)
                {
                    sb.AppendLine("==============INNER==============")
                        .AppendLine(FormatException(e.InnerException));
                    e = e.InnerException;
                }
            }

            sb.Append("=================================");
            return sb.ToString();
        }

        public static string ErrorLogBuilder(System.Exception e) => ErrorLogBuilder(e, false);

        public static string GetFormatString(this System.Exception e, bool showInnerException) =>
            ErrorLogBuilder(e, showInnerException);

        public static string GetFormatString(this System.Exception e) => ErrorLogBuilder(e);
    }
}
