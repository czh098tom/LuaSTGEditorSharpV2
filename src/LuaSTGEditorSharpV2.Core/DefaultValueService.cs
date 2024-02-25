using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core
{
    public class DefaultValueService : NodeServiceBase
    {
        [JsonProperty("DefaultValue")]
        private Dictionary<string, string> _defaultValues = [];

        internal protected virtual string GetValueWithDefault(NodeData dataSource
            , NodeContext context, string key, string defaultValue = "")
        {
            return dataSource.GetProperty(key, _defaultValues.GetValueOrDefault(key, defaultValue));
        }
    }
}
