using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public class BuildingContext
    {
        private Dictionary<string, string[]> _contextVariables = new();

        public string TemporaryFolderPath { get; set; } = Path.GetTempPath();
        public LocalParams LocalSettings { get; set; }

        public BuildingContext(LocalParams settings) 
        {
            LocalSettings = settings;
        }

        public void SetVariable(string key, string[] value) => _contextVariables.AddOrSet(key, value);

        public string[] GetVariables(string key) => _contextVariables.GetValueOrDefault(key, Array.Empty<string>());
    }
}
