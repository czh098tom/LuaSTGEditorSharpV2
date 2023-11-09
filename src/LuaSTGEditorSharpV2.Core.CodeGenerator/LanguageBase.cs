using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator
{
    public class LanguageBase
    {
        public static readonly LanguageBase _default = new();

        private static readonly Dictionary<string, LanguageBase> _cachedLanguages = new();

        public static void RegisterLanguage(string name, LanguageBase language) 
        {
            _cachedLanguages.Add(name, language);
        }

        public static LanguageBase? GetLanguage(string name)
        {
            if(_cachedLanguages.TryGetValue(name, out var language)) return language;
            return null;
        }

        public string? MacroTemplate { get; protected set; } = null;

        public string ApplyMacroWithString(string original, string toBeReplaced, string @new)
        {
            return GenerateRegexFor(toBeReplaced)?.Replace(original, @new) ?? original;
        }

        private Regex? GenerateRegexFor(string identifier)
        {
            if (MacroTemplate == null) return null;
            return new Regex(string.Format(MacroTemplate, identifier));
        }
    }
}
