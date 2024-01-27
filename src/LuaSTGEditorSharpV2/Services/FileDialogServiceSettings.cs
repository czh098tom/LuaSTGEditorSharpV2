using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Services
{
    public class FileDialogServiceSettings
    {
        [JsonProperty("open_file_path")]
        public string OpenFilePath { get; set; } = string.Empty;

        [JsonProperty("save_file_path")]
        public string SaveFilePath { get; set; } = string.Empty;
    }
}
