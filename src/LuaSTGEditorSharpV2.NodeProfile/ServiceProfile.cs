using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.NodeProfile
{
    public record class ServiceProfile(string Name, object Data, PackageInfo PackageInfo)
    {
    }
}
