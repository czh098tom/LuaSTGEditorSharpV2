using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public record struct EditResult(
        CommandBase? Command,
        bool ShouldRefreshView,
        LocalServiceParam LocalServiceParam)
    {
        public EditResult(LocalServiceParam localServiceParam) : this(null, false, localServiceParam) { }
        public EditResult(CommandBase? command, LocalServiceParam localServiceParam) : this(command, false, localServiceParam) { }
    }
}
