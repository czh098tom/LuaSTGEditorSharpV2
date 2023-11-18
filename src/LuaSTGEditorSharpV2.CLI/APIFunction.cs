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
        private static readonly Dictionary<string, APIFunction> _apis = new();

        public static void Register(string name, Action<APIFunctionParameter> execution, params Type[] services)
        {
            _apis.Add(name, new APIFunction(name, execution, services));
        }

        public static void FindAndExecute(string name, APIFunctionParameter param)
        {
            if (_apis.TryGetValue(name, out APIFunction? api))
            {
                api.Execute(param);
            }
        }

        public string Name { get; private set; }
        public IReadOnlyList<Type> Services { get; private set; }

        private Action<APIFunctionParameter> _execution;

        private APIFunction(string name, Action<APIFunctionParameter> execution, Type[] services)
        {
            Name = name;
            Services = services;
            _execution = execution;
        }

        public void Execute(APIFunctionParameter param)
        {
            var nodePackageProvider = HostedApplication.GetService<NodePackageProvider>();
            foreach (Type type in Services)
            {
                nodePackageProvider.UseService(type);
            }
            param.UsePackages();
            param.ReassignSettings();
            _execution(param);
        }
    }
}
