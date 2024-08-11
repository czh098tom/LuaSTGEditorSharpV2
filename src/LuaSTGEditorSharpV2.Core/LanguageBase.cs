using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    [PackagePrimaryKey(nameof(Name))]
    public class LanguageBase
    {
        public virtual string Name => "Neutral";

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
