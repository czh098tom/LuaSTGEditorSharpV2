using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.WPF.Converter;

namespace LuaSTGEditorSharpV2.UICustomization
{
    public class UICustomizationServiceSettings
    {
        [JsonProperty("uri")]
        public string Uri { get; private set; } = UICustomizationService.uri;

        [JsonProperty("inspector_font_size")]
        [ResourceDictionaryKey("inspector:text_size")]
        public double InspectorFontSize { get; private set; } = 15;

        [JsonProperty("inspector_font_family"), JsonConverter(typeof(StringFontFamilyConverter))]
        [ResourceDictionaryKey("inspector:font_family")]
        public FontFamily InspectorFontFamily { get; private set; } = new();
    }
}
