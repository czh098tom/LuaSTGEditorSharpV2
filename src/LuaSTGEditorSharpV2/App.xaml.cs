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

using LuaSTGEditorSharpV2.Core.Services;

using LuaSTGEditorSharpV2.ServiceInstanceProvider;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? host;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SplashWindow splash = new();
            splash.Show();

            await Task.Run(async () =>
            {
                host = new WPFApplicationHostBuilder(e.Args)
                    .BuildHost();
                await host.StartAsync();
                host.Services.GetRequiredService<LocalizationService>().OnCultureChanged += (o, e) =>
                    WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = e.CultureInfo;

                host.Services.GetRequiredService<NodePackageProvider>()
                    .Register(new SettingsDisplayDescriptorProvider());
            });

            MainWindow mw = host!.Services.GetRequiredService<MainWindow>();
            mw.Show();
            splash.Close();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await (host?.StopAsync() ?? Task.CompletedTask);
            base.OnExit(e);
        }
    }
}
