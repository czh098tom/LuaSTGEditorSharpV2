using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LuaSTGEditorSharpV2.Core
{
    [Serializable]
    public record class PackageManifest(string Name, 
        Version Version, 
        float Priority,
        string? LibraryPath)
    {
    }
}
