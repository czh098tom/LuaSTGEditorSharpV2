using LuaSTGEditorSharpV2.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LuaSTGEditorSharpV2.PropertyView
{
    [PackagePrimaryKey(nameof(Name))]
    public class ComboboxOptions(IServiceProvider serviceProvider) : PackedDataBase(serviceProvider)
    {
        public record OptionItem
        {
            [JsonProperty] public string Result { get; private set; } = string.Empty;
            [JsonProperty] public Dictionary<string, object?> Extra { get; private set; } = [];
        }

        [JsonProperty] public string Name { get; private set; } = string.Empty;
        [JsonProperty] public OptionItem?[] Options { get; private set; } = [];
    }
}
