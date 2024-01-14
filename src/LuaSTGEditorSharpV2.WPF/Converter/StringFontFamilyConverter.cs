using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.WPF.Converter
{
    public class StringFontFamilyConverter : JsonConverter<FontFamily>
    {
        public override FontFamily? ReadJson(JsonReader reader, Type objectType, FontFamily? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is not string s) return null;
            return new FontFamily(s);
        }

        public override void WriteJson(JsonWriter writer, FontFamily? value, JsonSerializer serializer)
        {
            if (value != null)
            {
                writer.WriteValue(value.ToString());
            }
            else
            {
                if (serializer.NullValueHandling == NullValueHandling.Include)
                {
                    writer.WriteNull();
                }
            }
        }
    }
}
