using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.CLI.Plugin
{
    [PackagePrimaryKey(nameof(Name))]
    public class CLIPluginDescriptor(string name, Func<APIFunctionParameter, Task> execution)
    {
        public string Name { get; private set; } = name;

        private readonly Func<APIFunctionParameter, Task> _execution = execution;

        public async Task Execute(APIFunctionParameter param)
        {
            param.ReassignSettings();
            await _execution(param);
        }
    }
}
