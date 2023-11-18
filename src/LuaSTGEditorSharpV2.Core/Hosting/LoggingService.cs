using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LuaSTGEditorSharpV2.Core.Hosting
{
    public class LoggingService
    {
        public static ILogger<LoggingService> Logger
            => HostedApplication.ApplicationHost.Services.GetService<LoggingService>()?._logger
            ?? throw new InvalidOperationException();

        private readonly ILogger<LoggingService> _logger;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
        }
    }
}
