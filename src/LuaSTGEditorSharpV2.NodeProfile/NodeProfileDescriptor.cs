using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.NodeProfile
{
    public class NodeProfileDescriptor(IServiceProvider serviceProvider) : NodeServiceBase(serviceProvider)
    {
        public string GetProfile() => string.Empty;
    }
}
