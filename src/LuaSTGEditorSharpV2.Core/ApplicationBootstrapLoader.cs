using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using LuaSTGEditorSharpV2.Core.Services;

namespace LuaSTGEditorSharpV2.Core
{
    public class ApplicationBootstrapLoader(PackedServiceCollection packedServices, Action<ILoggingBuilder> configureLogging)
    {
        public ServiceProvider Load()
        {
            ServiceCollection services = new();
            services.AddLogging(configureLogging);
            services.AddSingleton(_ => packedServices);
            services.AddSingleton<IServiceCollection>(_ => services);
            services.AddSingleton<SettingsService>();
            services.AddSingleton<BootstrapLoaderNodePackageProvider>();

            return services.BuildServiceProvider();
        }
    }
}
