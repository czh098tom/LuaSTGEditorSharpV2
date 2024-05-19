using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory.Configurable.InputSource
{
    public class SourceFromSettingsFactory : BuildTaskFactorySubService<IInputSourceVariable>
    {
        [JsonProperty] public NodePropertyCapture? SettingsJPathCapture { get; private set; }

        public override IInputSourceVariable CreateOutput(NodeData nodeData, BuildTaskFactoryContext context)
        {
            var token = new NodePropertyAccessToken(nodeData, context);
            return new SourceFromSettings(SettingsJPathCapture?.Capture(token) ?? string.Empty);
        }
    }
}
