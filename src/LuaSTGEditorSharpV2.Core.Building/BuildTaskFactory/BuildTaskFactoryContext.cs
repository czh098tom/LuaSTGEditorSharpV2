using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory
{
    public class BuildTaskFactoryContext : NodeContextWithSettings<BuildTaskFactoryServiceSettings>
    {
        public BuildTaskFactoryContext(LocalServiceParam localParam, BuildTaskFactoryServiceSettings serviceSettings) 
            : base(localParam, serviceSettings)
        {
        }
    }
}
