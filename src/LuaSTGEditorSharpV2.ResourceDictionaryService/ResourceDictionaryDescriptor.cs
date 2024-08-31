using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.ResourceDictionaryService
{
    [PackagePrimaryKey(nameof(Name))]
    public class ResourceDictionaryDescriptor(IServiceProvider serviceProvider) : PackedDataBase(serviceProvider)
    {
        public string Name { get; set; } = string.Empty;
        public string?[]? Uris { get; set; } = [];
    }
}
