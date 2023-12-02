using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaSTGEditorSharpV2.Core;
using Microsoft.Extensions.Hosting;

namespace LuaSTGEditorSharpV2.CLI
{
    public class MainWorker : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var args = HostedApplicationHelper.Args;
            APIFunctionRegistration.Register();
            var param = APIFunctionParameter.ParseFromCommandLineArgs(args);
            try
            {
                HostedApplicationHelper.InitNodeService();
                APIFunction.FindAndExecute(args[0], param);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            HostedApplicationHelper.ShutdownApplication();
            return Task.CompletedTask;
        }
    }
}
