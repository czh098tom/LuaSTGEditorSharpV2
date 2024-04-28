using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory
{
    public class BuildTaskFactoryServiceSettings : ServiceExtraSettingsBase
    {
        [JsonProperty("target_dir")]
        public string TargetDir { get; set; } = string.Empty;
    }
}
