using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.Package.Lua.CodeGenerator
{
    public class LuaLanguage : LanguageBase
    {
        public override string Name => "Lua";

        // TODO: Serialize
        internal LuaLanguage(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            MacroTemplate = "\\b{0}\\b"
            + @"(?<=^([^""]*((?<!(^|[^\\])(\\\\)*\\)""([^""]|((?<=(^|[^\\])(\\\\)*\\)""))*(?<!(^|[^\\])(\\\\)*\\)"")+)*[^""]*.)"
            + @"(?<=^([^']*((?<!(^|[^\\])(\\\\)*\\)'([^']|((?<=(^|[^\\])(\\\\)*\\)'))*(?<!(^|[^\\])(\\\\)*\\)')+)*[^']*.)";
        }
    }
}
