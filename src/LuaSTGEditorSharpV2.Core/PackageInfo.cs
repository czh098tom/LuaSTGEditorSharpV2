using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public record class PackageInfo(PackageManifest Manifest, string BasePath) 
        : IComparable<PackageInfo>
    {
        public int CompareTo(PackageInfo? other)
        {
            if (other == null) return 1;
            var priorityCmp = Manifest.Priority.CompareTo(other.Manifest.Priority);
            var versionCmp = Manifest.Version.CompareTo(other.Manifest.Version);
            return priorityCmp == 0 ? versionCmp : priorityCmp;
        }
    }
}
