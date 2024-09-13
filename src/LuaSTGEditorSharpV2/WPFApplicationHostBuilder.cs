using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using NLog.Extensions.Logging;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2
{
    internal class WPFApplicationHostBuilder(string[] args) : ApplicationHostBuilder(args)
    {
        protected override void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddNLog();
        }
    }
}
