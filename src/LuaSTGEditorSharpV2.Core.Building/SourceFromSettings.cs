using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public class SourceFromSettings : IInputSourceVariable
    {
        public string JPath { get; private set; }

        public SourceFromSettings(string jpath)
        {
            JPath = jpath;
        }

        public IReadOnlyList<string> GetVariable(BuildingContext context)
        {
            var str = (string?)context.GetSettingsFromJPath(JPath);
            if (str == null) return [];
            return [str];
        }
    }
}
