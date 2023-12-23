using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace LuaSTGEditorSharpV2.Core
{
    public static class LoggerHelper
    {
        public static void LogException(this ILogger logger, System.Exception e)
        {
            logger.LogError("Exception thrown with message \"{exception}\"", e.Message);
            logger.LogTrace("{stack_trace}", e.StackTrace);
        }
    }
}
