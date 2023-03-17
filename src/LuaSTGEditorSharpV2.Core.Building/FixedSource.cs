using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public class FixedSource : IInputSource
    {
        public string[] Path { get; private set; }

        public FixedSource(params string[] path)
        {
            Path = path;
        }

        public string[] GetSource(BuildingContext context)
        {
            return Path;
        }
    }
}
