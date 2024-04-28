using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public sealed class BuildingContext : IDisposable
    {
        private Dictionary<string, string[]> _contextVariables = new();

        public BuildingContexturalTemporaryFiles TempFiles { get; private set; } = new ();
        public LocalServiceParam LocalParam { get; private set; }

        private JObject _serviceSettings = [];
        private IReadOnlyDictionary<string, object> _serviceShortName2SettingsDict;

        public BuildingContext(BuildingContext source)
            : this(source.LocalParam, source._serviceShortName2SettingsDict)
        {
        }

        public BuildingContext(LocalServiceParam serviceParam, 
            IReadOnlyDictionary<string, object>? serviceShortName2SettingsDict = null) 
        {
            LocalParam = serviceParam;
            serviceShortName2SettingsDict ??= HostedApplicationHelper
                .GetService<NodePackageProvider>().GetServiceShortName2SettingsDict();
            _serviceShortName2SettingsDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(serviceShortName2SettingsDict))!;
            _serviceSettings = JsonConvert.DeserializeObject<JObject>(
                JsonConvert.SerializeObject(_serviceShortName2SettingsDict)) ?? [];
        }

        public void SetVariable(string key, string[] value) 
            => _contextVariables[key] = value;

        public IReadOnlyList<string> GetVariables(string key) 
            => _contextVariables.GetValueOrDefault(key, Array.Empty<string>());

        public JToken? GetSettingsFromJPath(string jpath)
        {
            return _serviceSettings.SelectToken(jpath);
        }

        public void Dispose()
        {
            TempFiles.Dispose();
        }
    }
}
