using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building
{
    /// <summary>
    /// Provide functionality of creating build commands from <see cref="NodeData"/>.
    /// </summary>
    [ServiceShortName("build"), ServiceName("BuildAction")]
    public class BuildActionServiceBase 
        : NodeService<BuildActionServiceProvider, BuildActionServiceBase, BuildActionContext, BuildActionServiceSettings>
    {
        public override sealed BuildActionContext GetEmptyContext(LocalServiceParam localSettings
            , BuildActionServiceSettings serviceSettings)
        {
            return new BuildActionContext(localSettings, serviceSettings);
        }

        public virtual async Task BuildWithContextAsync(NodeData node, BuildActionContext context)
        {
            await GetServiceProvider().ProceedChildrenAsync(node, context);
        }

        public void BuildWithContext(NodeData node, BuildActionContext context)
            => BuildWithContextAsync(node, context).RunSynchronously();
    }
}
