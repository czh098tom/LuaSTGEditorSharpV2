using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory.Configurable.OutputTarrget
{
    public class TargetToContextFactory : BuildTaskFactorySubService<IOutputTargetVariable>
    {
        [JsonProperty] public NodePropertyCapture? KeyCapture { get; private set; }

        public override IOutputTargetVariable CreateOutput(NodeData nodeData, BuildTaskFactoryContext context)
        {
            var token = new NodePropertyAccessToken(nodeData, context);
            return new TargetToContext(KeyCapture?.Capture(token) ?? string.Empty);
        }
    }
}
