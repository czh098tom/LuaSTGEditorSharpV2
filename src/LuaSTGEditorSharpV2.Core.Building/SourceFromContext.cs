using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public class SourceFromContext : IInputSource
    {
        public string Key { get; private set; }

        public SourceFromContext(string key)
        {
            Key = key;
        }

        public string[] GetSource(BuildingContext context)
        {
            return context.GetVariables(Key);
        }
    }
}
