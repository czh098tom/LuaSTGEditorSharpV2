using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LuaSTGEditorSharpV2.Core
{
    public static class HostedApplicationHelper
    {
        private static string[]? _args;

        private static IHost? _applicationHost;

        private static readonly List<Type> nodeServiceProviderTypes = [];
        private static readonly List<Type> applicationServiceProviderTypes = [];

        public static string[] Args => _args ?? throw new InvalidOperationException();

        public static void SetUpHost(Func<HostApplicationBuilder> builderFactory, string[] args)
        {
            _args = args;
            var builder = builderFactory();
            foreach (var type in nodeServiceProviderTypes)
            {
                builder.Services.AddSingleton(type);
            }
            foreach (var type in applicationServiceProviderTypes)
            {
                builder.Services.AddSingleton(type);
            }
            builder.Services.AddSingleton<NodePackageProvider>();
            _applicationHost = builder.Build();
            _applicationHost.RunAsync();
        }

        public static void InitNodeService()
        {
            foreach (var type in nodeServiceProviderTypes)
            {
                GetService<NodePackageProvider>().UseServiceProvider(type);
            }
        }

        public static void AddNodeServiceProvider<T>()
            where T : INodeServiceProvider
        {
            AddNodeServiceProvider(typeof(T));
        }

        public static void AddNodeServiceProvider(Type nodeServiceProvider)
        {
            nodeServiceProviderTypes.Add(nodeServiceProvider);
            applicationServiceProviderTypes.Add(nodeServiceProvider);
        }

        public static void AddApplicationSingletonService<T>()
            where T : class
        {
            AddApplicationSingletonService(typeof(T));
        }

        public static void AddApplicationSingletonService(Type serviceType)
        {
            applicationServiceProviderTypes.Add(serviceType);
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

        public static IEnumerable<T> GetServices<T>() where T : class
        {
            if (_applicationHost == null) throw new InvalidOperationException();
            return applicationServiceProviderTypes
                .Where(t => t.IsAnyDerivedTypeOf(typeof(T)))
                .Select(_applicationHost.Services.GetService)
                .OfType<T>() ?? throw new InvalidOperationException();
        }

        public static object GetService(Type type)
        {
            if (_applicationHost == null) throw new InvalidOperationException();
            return _applicationHost.Services.GetService(type)
                ?? throw new InvalidOperationException();
        }
    }
}
