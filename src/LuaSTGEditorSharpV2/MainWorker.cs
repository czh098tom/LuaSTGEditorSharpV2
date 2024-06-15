using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.ServiceBridge.Services;
using LuaSTGEditorSharpV2.ServiceBridge.CodeGenerator.ViewModel;
using LuaSTGEditorSharpV2.ResourceDictionaryService;
using LuaSTGEditorSharpV2.UICustomization;
using LuaSTGEditorSharpV2.ServiceBridge.UICustomization.ViewModel;
using LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory;
using LuaSTGEditorSharpV2.ServiceBridge.Building.ViewModel;

namespace LuaSTGEditorSharpV2
{
    public class MainWorker : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                SplashWindow? splash = null;
                do
                {
                    var windowsCollection = Application.Current.Windows;
                    splash = windowsCollection.OfType<SplashWindow>().FirstOrDefault();

                    await Task.Yield();
                }
                while (splash == null);

                HostedApplicationHelper.GetService<LocalizationService>().OnCultureChanged += (o, e) =>
                    WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = e.CultureInfo;

                await Task.Run(Initialization, stoppingToken);

                MainWindow mw = new();
                mw.Show();

                splash.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Application.Current.Shutdown();
            }
        }

        private void Initialization()
        {
            HostedApplicationHelper.GetService<SettingsService>().LoadSettings();

            HostedApplicationHelper.InitNodeService();
            var nodePackageProvider = HostedApplicationHelper.GetService<NodePackageProvider>();
            var resc = nodePackageProvider.LoadPackage("Core");
            var lua = nodePackageProvider.LoadPackage("Lua");
            var resln = nodePackageProvider.LoadPackage("LegacyNode");

            var settingsDisplay = HostedApplicationHelper.GetService<SettingsDisplayService>();
            settingsDisplay.RegisterViewModel<CodeGeneratorServiceProvider, CodeGenerationServiceSettingsViewModel>();
            settingsDisplay.RegisterViewModel<UICustomizationService, UICustomizationServiceSettingsViewModel>();
            settingsDisplay.RegisterViewModel<BuildTaskFactoryServiceProvider, BuildTaskFactoryServiceSettingsViewModel>();

            HostedApplicationHelper.GetService<ResourceDictionaryRegistrationService>().Init();
        }
    }
}
