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
        public IReadOnlyDictionary<string, JObject>? ServiceSettings { get; private set; }

        public APIFunctionParameter(IReadOnlyList<string>? packages, string? inputPath, string? outputPath
            , IReadOnlyDictionary<string, JObject>? serviceSettings)
        {
            Packages = packages;
            InputPath = inputPath;
            OutputPath = outputPath;
            ServiceSettings = serviceSettings;
        }

        public void UsePackages()
        {
            if (Packages == null) return;
            var nodePackageProvider = HostedApplication.GetService<NodePackageProvider>();
            foreach (var p in Packages)
            {
                nodePackageProvider.LoadPackage(p);
            }
        }

        public void ReassignSettings()
        {
            if (ServiceSettings == null) return;
            var nodePackageProvider = HostedApplication.GetService<NodePackageProvider>();
            foreach (var kvp in ServiceSettings)
            {
                nodePackageProvider.ReplaceSettingsForServiceShortNameIfValid(kvp.Key, kvp.Value);
            }
        }
    }
}
