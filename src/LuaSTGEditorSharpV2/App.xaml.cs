using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using NLog;
using NLog.Extensions.Logging;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Services;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.ServiceBridge.Services;
using LuaSTGEditorSharpV2.UICustomization;
using LuaSTGEditorSharpV2.ResourceDictionaryService;
using LuaSTGEditorSharpV2.Core.Command.Service;
using LuaSTGEditorSharpV2.Toolbox.Service;
using LuaSTGEditorSharpV2.Toolbox.Model;

using static LuaSTGEditorSharpV2.Core.HostedApplicationHelper;
using LuaSTGEditorSharpV2.Core.Building.ResourceGathering;
using LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory;
using LuaSTGEditorSharpV2.ServiceBridge;

namespace LuaSTGEditorSharpV2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var args = e.Args;

            PackedServiceCollection packedServiceInfos = new();

            packedServiceInfos.Add<CodeGeneratorServiceProvider>();
            packedServiceInfos.Add<ViewModelProviderServiceProvider>();
            packedServiceInfos.Add<ResourceGatheringServiceProvider>();
            packedServiceInfos.Add<BuildTaskFactoryServiceProvider>();
            packedServiceInfos.Add<PropertyViewServiceProvider>();
            packedServiceInfos.Add<DefaultValueServiceProvider>();
            packedServiceInfos.Add<ToolboxProviderService>();
            packedServiceInfos.Add<ResourceDictionaryRegistrationService>();
            packedServiceInfos.Add<LanguageProviderService>();
            packedServiceInfos.Add<SettingsDisplayService>();

            ApplicationBootstrapLoader applicationBootstrapLoader = new(packedServiceInfos, builder =>
            {
                builder.AddNLog();
            });

            IReadOnlyCollection<PackageAssemblyDescriptor> assemblyDesc = [];
            using (var bootStrap = applicationBootstrapLoader.Load())
            {
                var bootStrapService = bootStrap.GetRequiredService<BootstrapLoaderNodePackageProvider>();
                assemblyDesc = bootStrapService.LoadAssemblies();
            }

            foreach(var info in packedServiceInfos)
            {
                AddPackedDataProvider(info.ServiceProviderType);
            }

            AddApplicationSingletonService<SettingsService>();
            AddApplicationSingletonService<ActiveDocumentService>();
            AddApplicationSingletonService<InsertCommandHostingService>();
            AddApplicationSingletonService<LocalizationService>();
            AddApplicationSingletonService<UICustomizationService>();
            AddApplicationSingletonService<MainWindowLayoutService>();
            AddApplicationSingletonService<FileDialogService>();
            AddApplicationSingletonService<ClipboardService>();
            SetUpHost(() =>
            {
                HostApplicationBuilder applicationBuilder = Host.CreateApplicationBuilder(args);

                applicationBuilder.Services.AddSingleton<IPackedServiceCollection>(_ => packedServiceInfos);
                applicationBuilder.Services.AddSingleton(_ => assemblyDesc);
                applicationBuilder.Services.AddLogging(builder => builder.AddNLog());
                applicationBuilder.Services.AddHostedService<MainWorker>();
                return applicationBuilder;
            }, args);

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await ExitApplicationAsync();
            base.OnExit(e);
        }
    }
}
