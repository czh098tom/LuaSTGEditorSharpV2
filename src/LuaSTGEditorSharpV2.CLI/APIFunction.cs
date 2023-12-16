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

        public static void Register(string name, Action<APIFunctionParameter> execution)
        {
            _apis.Add(name, new APIFunction(name, execution));
        }

        public static void FindAndExecute(string name, APIFunctionParameter param)
        {
            if (_apis.TryGetValue(name, out APIFunction? api))
            {
                api.Execute(param);
            }
        }

        public string Name { get; private set; }

        private Action<APIFunctionParameter> _execution;

        private APIFunction(string name, Action<APIFunctionParameter> execution)
        {
            Name = name;
            _execution = execution;
        }

        public void Execute(APIFunctionParameter param)
        {
            var nodePackageProvider = HostedApplicationHelper.GetService<NodePackageProvider>();
            param.UsePackages();
            param.ReassignSettings();
            _execution(param);
        }
    }
}
