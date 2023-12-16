using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public abstract record class Localizable<T>(
        T? Neutral = null,
        Dictionary<string, T>? Localized = null)
        where T : class
    {
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

    public record class LocalizableString : Localizable<string>
    {
        public override string Default => string.Empty;
    }

    public record class LocalizableArray<T> : Localizable<T[]>
    {
        public override T[] Default => Array.Empty<T>();
    }
}
