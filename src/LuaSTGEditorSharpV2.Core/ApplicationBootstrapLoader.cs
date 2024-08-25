using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaSTGEditorSharpV2.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LuaSTGEditorSharpV2.Core
{
    public class ApplicationBootstrapLoader(PackedServiceCollection packedServices, Action<ILoggingBuilder> configureLogging)
    {
        public ServiceProvider Load()
        {
            ServiceCollection services = new();
            services.AddLogging(configureLogging);
            services.AddSingleton(_ => packedServices);
            services.AddSingleton<SettingsService>();
            services.AddSingleton<BootstrapLoaderNodePackageProvider>();

            return services.BuildServiceProvider();
        }
    }
}
