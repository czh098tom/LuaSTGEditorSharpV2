using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LuaSTGEditorSharpV2.Core
{
    public static class HostedApplication
    {
        private static string[]? _args;

        private static IHost? _applicationHost;

        public static string[] Args => _args ?? throw new InvalidOperationException();

        public static void SetUpHost(Func<HostApplicationBuilder> builderFactory, string[] args)
        {
            _args = args;
            var builder = builderFactory();
            builder.Services.AddSingleton<NodePackageProvider>();
            _applicationHost = builder.Build();
            _applicationHost.RunAsync();
        }

        public static void ShutdownApplication()
        {
            _applicationHost?.StopAsync();
        }

        public static T GetService<T>() where T : class
        {
            if (_applicationHost == null) throw new InvalidOperationException();
            return _applicationHost.Services.GetService<T>() 
                ?? throw new InvalidOperationException();
        }
    }
}
