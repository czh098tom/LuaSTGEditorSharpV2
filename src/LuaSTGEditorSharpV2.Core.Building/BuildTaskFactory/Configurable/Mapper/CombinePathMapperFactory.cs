using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory.Configurable.Mapper
{
    public class CombinePathMapperFactory : BuildTaskFactorySubService<Func<string, string>>
    {
        [JsonProperty] public NodePropertyCapture? ToAppendCapture { get; private set; }

        public override Func<string, string> CreateOutput(NodeData nodeData, BuildTaskFactoryContext context)
        {
            var token = new NodePropertyAccessToken(nodeData, context);
            var arg = ToAppendCapture?.Capture(token) ?? string.Empty;
            return s => Path.Combine(s, arg);
        }
    }
}
