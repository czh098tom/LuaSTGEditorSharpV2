﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.CLI.Plugin
{
    [PackedServiceProvider]
    [ServiceName("CLIPluginProvider")]
    public class CLIPluginProviderService(IServiceProvider serviceProvider) : PackedDataProviderServiceBase<CLIPluginDescriptor>(serviceProvider)
    {
        public async Task FindAndExecute(string name, APIFunctionParameter param)
        {
            await (GetDataOfID(name)?.Execute(ServiceProvider, param) ?? Task.CompletedTask);
        }
    }
}
