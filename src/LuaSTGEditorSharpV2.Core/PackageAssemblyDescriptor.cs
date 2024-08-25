using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public record class PackageAssemblyDescriptor(
        string Name,
        string BasePath,
        PackageManifest Manifest,
        IReadOnlyCollection<Assembly> Assemblies)
    {
    }
}
