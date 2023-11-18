using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HostedApplication.SetUpHost(() =>
            {
                HostApplicationBuilder applicationBuilder = Host.CreateApplicationBuilder(args);

                applicationBuilder.Services.AddLogging(builder => builder.AddConsole());
                applicationBuilder.Services.AddHostedService<MainWorker>();
                return applicationBuilder;
            }, args);
        }
    }
}