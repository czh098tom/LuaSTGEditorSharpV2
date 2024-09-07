using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.CLI.Plugin;
using LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.CLIPluginProvider
{
    public class CLIPluginDescriptorProvider : IServiceInstanceProvider<CLIPluginDescriptor>
    {
        public IReadOnlyCollection<CLIPluginDescriptor> GetServiceInstances(IServiceProvider serviceProvider)
        {
            var desc = new CLIPluginDescriptor("sharpconv", async param =>
            {
                var inputPath = param.InputPath;
                var outputPath = param.OutputPath;
                if (inputPath == null || outputPath == null) return;
                var converter = serviceProvider.GetRequiredService<SharpProjectConverterFactory>().Create(inputPath, outputPath);
                try
                {
                    await converter.Convert(default);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }, serviceProvider);
            return [desc];
        }
    }
}
