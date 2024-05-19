using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory.Configurable
{
    public class BuildTaskPropertySubService : BuildTaskFactorySubService<string>
    {
        [JsonProperty] public NodePropertyCapture? PropertyNameCapture { get; private set; }

        public override string CreateOutput(NodeData nodeData, BuildTaskFactoryContext context)
        {
            var token = new NodePropertyAccessToken(nodeData, context);
            return PropertyNameCapture?.Capture(token) ?? string.Empty;
        }
    }
}
