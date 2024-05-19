using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory.Configurable.InputSource
{
    public class ConstantSourceFactory : BuildTaskFactorySubService<IInputSourceVariable>
    {
        [JsonProperty] public NodePropertyCapture? PathCapture { get; private set; }

        public override IInputSourceVariable CreateOutput(NodeData nodeData, BuildTaskFactoryContext context)
        {
            var token = new NodePropertyAccessToken(nodeData, context);
            return new ConstantSource(PathCapture?.Capture(token) ?? string.Empty);
        }
    }
}
