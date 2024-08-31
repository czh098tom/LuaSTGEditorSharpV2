using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.CLI.Plugin
{
    public class APIFunctionParameter
    {
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

        public void ReassignSettings(IServiceProvider serviceProvider)
        {
            if (ServiceSettings == null) return;
            var nodePackageProvider = serviceProvider.GetRequiredService<NodePackageProvider>();
            foreach (var kvp in ServiceSettings)
            {
                nodePackageProvider.ReplaceSettingsForServiceShortNameIfValid(kvp.Key, kvp.Value);
            }
        }
    }
}
