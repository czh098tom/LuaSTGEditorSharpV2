using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LuaSTGEditorSharpV2.Core.Hosting
{
    public static class HostedApplication
    {
        private static string[]? _args;

        private static IHost? _applicationHost;
        public static IHost ApplicationHost => _applicationHost ?? throw new InvalidOperationException();

        public static string[] Args => _args ?? throw new InvalidOperationException();

        public static void SetUpHost(Func<HostApplicationBuilder> builderFactory, string[] args)
        {
            _args = args;
            var builder = builderFactory();
            _applicationHost = builder.Build();
            _applicationHost.RunAsync();
        }

        public static void ShutdownApplication()
        {
            _applicationHost?.StopAsync();
        }
    }
}
