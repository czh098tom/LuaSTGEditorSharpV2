using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory;
using LuaSTGEditorSharpV2.Core.Building.ResourceGathering;

using static LuaSTGEditorSharpV2.Core.HostedApplicationHelper;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.CLI.Plugin;

namespace LuaSTGEditorSharpV2.CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AddPackedDataProvider(typeof(DefaultValueServiceProvider));
            AddPackedDataProvider(typeof(CodeGeneratorServiceProvider));
            AddPackedDataProvider(typeof(BuildTaskFactoryServiceProvider));
            AddPackedDataProvider(typeof(ResourceGatheringServiceProvider));
            AddPackedDataProvider(typeof(CLIPluginProviderService));
            AddApplicationSingletonService<SettingsService>();
            AddApplicationSingletonService(typeof(LanguageProviderService));
            SetUpHost(() =>
            {
                HostApplicationBuilder applicationBuilder = Host.CreateApplicationBuilder(args);

                applicationBuilder.Services.AddLogging(builder => builder.AddConsole());
                applicationBuilder.Services.AddHostedService<MainWorker>();
                return applicationBuilder;
            }, args);

            WaitForShutdown();
        }
    }
}