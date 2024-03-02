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
    public record class PackageInfo(string Name, 
        Version Version, 
        float Priority,
        string? LibraryPath) : IComparable<PackageInfo>
    {
        public int CompareTo(PackageInfo? other)
        {
            if (other == null) return 1;
            var priorityCmp = Priority.CompareTo(other.Priority);
            var versionCmp = Priority.CompareTo(other.Version);
            return priorityCmp == 0 ? Version.CompareTo(other.Version) : versionCmp;
        }
    }
}
