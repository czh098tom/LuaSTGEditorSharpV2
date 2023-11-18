using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaSTGEditorSharpV2.Core.Hosting;
using Microsoft.Extensions.Hosting;

namespace LuaSTGEditorSharpV2.CLI
{
    public class MainWorker : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var args = HostedApplication.Args;
            APIFunctionRegistration.Register();
            var param = APIFunctionParameter.ParseFromCommandLineArgs(args);
            try
            {
                APIFunction.FindAndExecute(args[0], param);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            HostedApplication.ShutdownApplication();
            return Task.CompletedTask;
        }
    }
}
