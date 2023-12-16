using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public record class LocalizableString(
        string? Neutral = null,
        Dictionary<string, string>? Localized = null)
    {
        public string GetLocalized(string @default = "")
        {
            var str = Neutral ?? string.Empty;
            if (Localized == null)
            {
                return str;
            }
            else
            {
                return Localized?.GetValueOrDefault(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
                    , str) ?? @default;
            }
        }
    }
}
