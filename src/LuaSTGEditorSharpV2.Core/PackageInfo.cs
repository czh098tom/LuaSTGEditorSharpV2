using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    [Serializable]
    public record class PackageInfo(string Name, int VersionNumber, string? LibraryPath) : IComparable<PackageInfo>
    {
        public int CompareTo(PackageInfo? other)
        {
            if (other == null) return 1;
            return VersionNumber.CompareTo(other.VersionNumber);
        }
    }
}
