using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public class TargetToContext : IOutputTargetVariable
    {
        public string Key { get; private set; }

        public TargetToContext(string key)
        {
            Key = key;
        }

        public void WriteTarget(IReadOnlyList<string> target, BuildingContext context)
        {
            context.SetVariable(Key, [.. target]);
        }
    }
}
