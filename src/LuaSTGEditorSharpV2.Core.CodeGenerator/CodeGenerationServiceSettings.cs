using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Settings;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator
{
    public class CodeGenerationServiceSettings : ServiceExtraSettingsBase
    {
        [JsonProperty("indention_string")] 
        public string IndentionString { get; set; } = "\t";
        [JsonProperty("indent_on_blank_line")]
        public bool IndentOnBlankLine { get; set; } = false;
        [JsonProperty("skip_blank_line")]
        public bool SkipBlankLine { get; set; } = false;
        [JsonProperty("line_obfuscated")]
        public bool LineObfuscated { get; set; } = false;
    }
}
