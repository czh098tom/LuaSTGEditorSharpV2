using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public class LanguageProviderService
    {
        public readonly LanguageBase _default = new();

        private readonly Dictionary<string, LanguageBase> _cachedLanguages = new();

        public void RegisterLanguage(string name, LanguageBase language)
        {
            _cachedLanguages.Add(name, language);
        }

        public LanguageBase? GetLanguage(string name)
        {
            if (_cachedLanguages.TryGetValue(name, out var language)) return language;
            return null;
        }
    }
}
