using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using NLog.Extensions.Logging;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory;
using LuaSTGEditorSharpV2.Core.Building.ResourceGathering;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.ResourceDictionaryService;
using LuaSTGEditorSharpV2.ServiceBridge.Services;
using LuaSTGEditorSharpV2.Toolbox.Service;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Core.Command.Service;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.Services;
using LuaSTGEditorSharpV2.UICustomization;

namespace LuaSTGEditorSharpV2
{
    internal class WPFApplicationHostBuilder(string[] args) : ApplicationHostBuilder(args)
    {
        protected override void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddNLog();
        }

        protected override void ConfigurePackedServiceInfos(PackedServiceCollection collection)
        {
            base.ConfigurePackedServiceInfos(collection);

            collection.Add<CodeGeneratorServiceProvider>();
            collection.Add<ViewModelProviderServiceProvider>();
            collection.Add<ResourceGatheringServiceProvider>();
            collection.Add<BuildTaskFactoryServiceProvider>();
            collection.Add<PropertyViewServiceProvider>();
            collection.Add<DefaultValueServiceProvider>();
            collection.Add<ToolboxProviderService>();
            collection.Add<ResourceDictionaryRegistrationService>();
            collection.Add<LanguageProviderService>();
            collection.Add<SettingsDisplayService>();
        }

        protected override void ConfigureService(IServiceCollection services)
        {
            base.ConfigureService(services);

            //services.AddSingleton<ActiveDocumentService>();
            //services.AddSingleton<InsertCommandHostingService>();
            //services.AddSingleton<LocalizationService>();
            //services.AddSingleton<UICustomizationService>();
            //services.AddSingleton<MainWindowLayoutService>();
            //services.AddSingleton<FileDialogService>();
            //services.AddSingleton<ClipboardService>();
        }
    }
}
