﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.CLI.Plugin;
using LuaSTGEditorSharpV2.CLI.ServiceInstanceProvider;

namespace LuaSTGEditorSharpV2.CLI
{
    public class MainWorker : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var args = HostedApplicationHelper.Args;
            var param = APIFunctionParameterResolver.ParseFromCommandLineArgs(args);
            HostedApplicationHelper.InitNodeService();
            param.UsePackages();
            HostedApplicationHelper.GetService<NodePackageProvider>().Register(new CLIPluginDescriptorProvider());
            HostedApplicationHelper.GetService<SettingsService>().LoadSettings();
            try
            {
                await HostedApplicationHelper.GetService<CLIPluginProviderService>().FindAndExecute(args[0], param);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            HostedApplicationHelper.ShutdownApplication();
        }
    }
}
