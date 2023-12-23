using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core
{
    public class NodePropertyCapture
    {
        [JsonProperty] public string Key { get; private set; } = string.Empty;
        [JsonProperty] public string DefaultValue { get; private set; } = string.Empty;

        public string Capture(NodeData nodeData)
        {
            return nodeData.GetProperty(Key, DefaultValue);
        }

        public string CaptureByFormat(NodeData nodeData, object? arg0)
        {
            return nodeData.GetProperty(string.Format(Key, arg0), DefaultValue);
        }

        public string CaptureByFormat(NodeData nodeData, params object?[] args)
        {
            return nodeData.GetProperty(string.Format(Key, args), DefaultValue);
        }
    }
}
