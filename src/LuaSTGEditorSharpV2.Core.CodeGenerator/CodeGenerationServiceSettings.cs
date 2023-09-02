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
    }
}
