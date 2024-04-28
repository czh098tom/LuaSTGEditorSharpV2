using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public class BuildingSession
    {
        public BuildingContext Context { get; set; }

        public BuildingSession(LocalServiceParam localServiceParam) 
        {
            Context = new BuildingContext(localServiceParam);
        }
    }
}
