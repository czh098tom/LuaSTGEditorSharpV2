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

namespace LuaSTGEditorSharpV2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var args = e.Args;

            HostedApplicationHelper.AddNodeServiceProvider(typeof(CodeGeneratorServiceProvider));
            HostedApplicationHelper.AddNodeServiceProvider(typeof(ViewModelProviderServiceProvider));
            HostedApplicationHelper.AddNodeServiceProvider(typeof(PropertyViewServiceProvider));
            HostedApplicationHelper.SetUpHost(() =>
            {
                HostApplicationBuilder applicationBuilder = Host.CreateApplicationBuilder(args);

                applicationBuilder.Services.AddLogging(builder => builder.AddNLog());
                applicationBuilder.Services.AddHostedService<MainWorker>();
                applicationBuilder.Services.AddSingleton<ActiveDocumentService>();
                applicationBuilder.Services.AddSingleton<LocalizationService>();
                return applicationBuilder;
            }, args);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
