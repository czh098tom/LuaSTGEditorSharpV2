using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory;
using LuaSTGEditorSharpV2.Core.Building.ResourceGathering;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.CLI.Plugin;

using static LuaSTGEditorSharpV2.Core.HostedApplicationHelper;

namespace LuaSTGEditorSharpV2.CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            PackedServiceCollection packedServiceInfos = new();

            packedServiceInfos.Add<DefaultValueServiceProvider>();
            packedServiceInfos.Add<CodeGeneratorServiceProvider>();
            packedServiceInfos.Add<BuildTaskFactoryServiceProvider>();
            packedServiceInfos.Add<ResourceGatheringServiceProvider>();
            packedServiceInfos.Add<CLIPluginProviderService>();
            
            ApplicationBootstrapLoader applicationBootstrapLoader = new(packedServiceInfos, builder =>
            {
                builder.AddConsole();
            });

            var param = APIFunctionParameterResolver.ParseFromCommandLineArgs(args);

            IReadOnlyCollection<PackageAssemblyDescriptor> assemblyDesc = [];
            using (var bootStrap = applicationBootstrapLoader.Load())
            {
                var bootStrapService = bootStrap.GetRequiredService<BootstrapLoaderNodePackageProvider>();

                var settingsService = bootStrap.GetRequiredService<SettingsService>();
                settingsService.LoadSettings();

                assemblyDesc = param.Packages
                    ?.Select(bootStrapService.LoadAssembly)
                    ?.OfType<PackageAssemblyDescriptor>()
                    ?.ToArray() ?? bootStrapService.LoadAssemblies();
            }

            foreach (var info in packedServiceInfos)
            {
                AddPackedDataProvider(info.ServiceProviderType);
            }

            AddApplicationSingletonService<SettingsService>();
            AddApplicationSingletonService(typeof(LanguageProviderService));
            SetUpHost(() =>
            {
                HostApplicationBuilder applicationBuilder = Host.CreateApplicationBuilder(args);

                applicationBuilder.Services.AddSingleton<IPackedServiceCollection>(_ => packedServiceInfos);
                applicationBuilder.Services.AddSingleton(_ => assemblyDesc);
                applicationBuilder.Services.AddLogging(builder => builder.AddConsole());
                applicationBuilder.Services.AddHostedService<MainWorker>();
                return applicationBuilder;
            }, args);

            WaitForShutdown();
        }
    }
}