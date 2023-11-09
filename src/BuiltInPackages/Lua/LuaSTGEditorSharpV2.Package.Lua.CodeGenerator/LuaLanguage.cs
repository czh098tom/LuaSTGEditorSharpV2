using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.CodeGenerator;

namespace LuaSTGEditorSharpV2.Package.Lua.CodeGenerator
{
    public class LuaLanguage : LanguageBase
    {
        // TODO: Serialize
        internal LuaLanguage()
        {
            MacroTemplate = "\\b{0}\\b"
            + @"(?<=^([^""]*((?<!(^|[^\\])(\\\\)*\\)""([^""]|((?<=(^|[^\\])(\\\\)*\\)""))*(?<!(^|[^\\])(\\\\)*\\)"")+)*[^""]*.)"
            + @"(?<=^([^']*((?<!(^|[^\\])(\\\\)*\\)'([^']|((?<=(^|[^\\])(\\\\)*\\)'))*(?<!(^|[^\\])(\\\\)*\\)')+)*[^']*.)";
        }
    }
}
