using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.CLI.Plugin;
using LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory;
using LuaSTGEditorSharpV2.Core.Building.ResourceGathering;
using LuaSTGEditorSharpV2.Core.CodeGenerator;

namespace LuaSTGEditorSharpV2.CLI
{
    public class CLIApplicationHostBuilder(string[] args) : ApplicationHostBuilder(args)
    {
        protected override void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddConsole();
        }

        protected override IReadOnlyCollection<string>? OverridePackages()
        {
            return APIFunctionParameterResolver.ParseFromCommandLineArgs(_args).Packages;
        }
    }
}
