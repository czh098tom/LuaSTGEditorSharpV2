using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.UICustomization
{
    public class UICustomizationServiceSettings
    {
        [JsonProperty("uri")]
        public string Uri { get; private set; } = UICustomizationService.uri;

        [JsonProperty("inspector_font_size")]
        [ResourceDictionaryKey("inspector:text_size")]
        public double InspectorFontSize { get; private set; } = 15;
    }
}
