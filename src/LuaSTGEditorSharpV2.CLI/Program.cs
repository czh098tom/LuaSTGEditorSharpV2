using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory;
using LuaSTGEditorSharpV2.Core.Building.ResourceGathering;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.CLI.Plugin;

using LuaSTGEditorSharpV2.CLI.ServiceInstanceProvider;

namespace LuaSTGEditorSharpV2.CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = new CLIApplicationHostBuilder(args)
                .BuildHost();
            host.Services.GetRequiredService<NodePackageProvider>().Register(
                host.Services.GetRequiredService<CLIPluginDescriptorProvider>());
            var param = APIFunctionParameterResolver.ParseFromCommandLineArgs(args);
            try
            {
                host.Services.GetRequiredService<CLIPluginProviderService>().FindAndExecute(args[0], param).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}