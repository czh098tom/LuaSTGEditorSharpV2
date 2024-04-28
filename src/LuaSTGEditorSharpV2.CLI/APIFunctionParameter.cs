using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.CLI
{
    public class APIFunctionParameter
    {
        public static APIFunctionParameter ParseFromCommandLineArgs(string[] args)
        {
            return new APIFunctionParameterResolver().Resolve(args);
        }

        public IReadOnlyList<string>? Packages { get; private set; }
        public string? InputPath { get; private set; }
        public string? OutputPath { get; private set; }
        public string? TaskName { get; private set; }
        public IReadOnlyDictionary<string, JObject>? ServiceSettings { get; private set; }

        public APIFunctionParameter(BuiltInParams builtInParams
            , IReadOnlyDictionary<string, JObject>? serviceSettings)
        {
            Packages = builtInParams.Packages;
            InputPath = builtInParams.InputPath;
            OutputPath = builtInParams.OutputPath;
            TaskName = builtInParams.TaskName;
            ServiceSettings = serviceSettings;
        }

        public void UsePackages()
        {
            if (Packages == null) return;
            var nodePackageProvider = HostedApplicationHelper.GetService<NodePackageProvider>();
            foreach (var p in Packages)
            {
                nodePackageProvider.LoadPackage(p);
            }
        }

        public void ReassignSettings()
        {
            if (ServiceSettings == null) return;
            var nodePackageProvider = HostedApplicationHelper.GetService<NodePackageProvider>();
            foreach (var kvp in ServiceSettings)
            {
                nodePackageProvider.ReplaceSettingsForServiceShortNameIfValid(kvp.Key, kvp.Value);
            }
        }
    }
}
