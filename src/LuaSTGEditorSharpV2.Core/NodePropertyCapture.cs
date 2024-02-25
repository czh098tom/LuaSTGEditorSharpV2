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

        public string Capture(NodePropertyAccessToken token)
        {
            return token.GetValueWithDefault(Key, DefaultValue);
        }

        public string CaptureByFormat(NodePropertyAccessToken token, object? arg0)
        {
            return token.GetValueWithDefault(string.Format(Key, arg0), DefaultValue);
        }

        public string CaptureByFormat(NodePropertyAccessToken token, params object?[] args)
        {
            return token.GetValueWithDefault(string.Format(Key, args), DefaultValue);
        }
    }
}
