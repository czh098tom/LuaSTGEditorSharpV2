using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Execution
{
    public class ExecutionConfigServiceSettings
    {
        [JsonProperty("target_executable")]
        public string? TargetExecutablePath { get; set; } = string.Empty;
    }
}
