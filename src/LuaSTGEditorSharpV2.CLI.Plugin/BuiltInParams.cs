using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.CLI.Plugin
{
    public record class BuiltInParams(
            string[]? Packages,
            string? InputPath,
            string? OutputPath,
            string? TaskName
            ) { }
}
