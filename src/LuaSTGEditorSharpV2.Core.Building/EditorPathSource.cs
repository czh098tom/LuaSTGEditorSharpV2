using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public class EditorPathSource : IInputSourceVariable
    {
        public IReadOnlyList<string> GetVariable(BuildingContext context)
        {
            var path = Assembly.GetEntryAssembly()?.Location;

            return [Path.GetDirectoryName(path) ?? string.Empty];
        }
    }
}
