using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.ResourceDictionaryService
{
    [PackagePrimaryKey(nameof(Name))]
    public class ResourceDictionaryDescriptor(IServiceProvider serviceProvider) : PackedDataBase(serviceProvider)
    {
        [JsonProperty] public string Name { get; private set; } = string.Empty;
        [JsonProperty] public string?[]? Uris { get; private set; } = [];
    }
}
