using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.CLI
{
    public class APIFunction
    {
        private static readonly Dictionary<string, APIFunction> _apis = [];

        public static void Register(string name, Func<APIFunctionParameter, Task> execution)
        {
            _apis.Add(name, new APIFunction(name, execution));
        }

        public static async Task FindAndExecute(string name, APIFunctionParameter param)
        {
            if (_apis.TryGetValue(name, out APIFunction? api))
            {
                await api.Execute(param);
            }
        }

        public string Name { get; private set; }

        private Func<APIFunctionParameter, Task> _execution;

        private APIFunction(string name, Func<APIFunctionParameter, Task> execution)
        {
            Name = name;
            _execution = execution;
        }

        public async Task Execute(APIFunctionParameter param)
        {
            var nodePackageProvider = HostedApplicationHelper.GetService<NodePackageProvider>();
            param.UsePackages();
            param.ReassignSettings();
            await _execution(param);
        }
    }
}
