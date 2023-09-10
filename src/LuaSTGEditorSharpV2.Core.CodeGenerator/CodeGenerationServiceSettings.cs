using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator
{
    public class CodeGenerationServiceSettings : ServiceExtraSettings<CodeGenerationServiceSettings>
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
