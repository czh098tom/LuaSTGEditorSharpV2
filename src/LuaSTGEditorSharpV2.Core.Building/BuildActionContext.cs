using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public class BuildActionContext : NodeContextWithSettings<BuildActionServiceSettings>
    {
        public LocalServiceParam LocalSettings => base.LocalParam;

        public BuildActionContext(LocalServiceParam localSettings, BuildActionServiceSettings serviceSettings) 
            : base(localSettings, serviceSettings)
        {
        }
    }
}
