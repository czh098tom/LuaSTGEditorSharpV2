using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public abstract class Localizable<T>
        where T : class
    {
        [JsonProperty] public T? Neutral { get; private set; } = null;
        [JsonProperty] public Dictionary<string, T>? Localized { get; private set; } = null;

        [JsonIgnore] public abstract T Default { get; }

        public T GetLocalized(T? @default = null)
        {
            @default ??= Default;
            var str = Neutral ?? Default;
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

    public class LocalizableString
        : Localizable<string>
    {
        public override string Default => string.Empty;
    }

    public class LocalizableArray<T>
        : Localizable<T[]>
    {
        public override T[] Default => Array.Empty<T>();
    }
}
