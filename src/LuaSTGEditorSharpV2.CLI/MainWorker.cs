using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Services;

namespace LuaSTGEditorSharpV2.CLI
{
    public class MainWorker : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var args = HostedApplicationHelper.Args;
            APIFunctionRegistration.Register();
            var param = APIFunctionParameter.ParseFromCommandLineArgs(args);
            try
            {
                HostedApplicationHelper.InitNodeService();
                HostedApplicationHelper.GetService<SettingsService>().LoadSettings();
                await APIFunction.FindAndExecute(args[0], param);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            HostedApplicationHelper.ShutdownApplication();
        }
    }
}
