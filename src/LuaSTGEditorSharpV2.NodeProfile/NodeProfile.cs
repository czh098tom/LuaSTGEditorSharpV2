using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.NodeProfile
{
    public record class NodeProfile(string Name, IReadOnlyList<ServiceProfile> Profiles)
    {
    }
}
