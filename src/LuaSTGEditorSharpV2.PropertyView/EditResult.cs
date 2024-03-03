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
        bool ShouldRefreshView)
    {
        public static readonly EditResult Empty = new();

        public EditResult() : this(null, false) { }
        public EditResult(CommandBase? command) : this(command, false) { }
    }
}
