using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public static class LocalizedHelper
    {
        public static string GetI18NValueOrDefault(this Dictionary<string, string> dictionary
            , string @default)
        {
            return dictionary.GetValueOrDefault(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
                , @default);
        }
    }
}
