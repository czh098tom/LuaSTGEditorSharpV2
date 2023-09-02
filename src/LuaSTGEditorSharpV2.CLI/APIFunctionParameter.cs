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
        public string[]? Packages { get; set; }
        public string? InputPath { get; set; }
        public string? OutputPath { get; set; }
        public IReadOnlyDictionary<string, JObject>? ServiceSettings { get; set; }

        public void UsePackages()
        {
            if (Packages == null) return;
            foreach (var p in Packages)
            {
                ServiceManager.LoadPackage(p);
            }
        }

        public void ReassignSettings()
        {
            if (ServiceSettings == null) return;
            foreach (var kvp in ServiceSettings)
            {
                ServiceManager.ReplaceSettingsForServiceShortName(kvp.Key, kvp.Value);
            }
        }
    }
}
