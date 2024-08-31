using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory
{
    public class BuildTaskFactoryContext : NodeContextWithSettings<BuildTaskFactoryServiceSettings>
    {
        public BuildTaskFactoryContext(IServiceProvider serviceProvider, LocalServiceParam localParam, BuildTaskFactoryServiceSettings serviceSettings)
            : base(serviceProvider, localParam, serviceSettings)
        {
        }
    }
}
