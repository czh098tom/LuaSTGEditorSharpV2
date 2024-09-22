using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Serialization
{
    internal record class SerializationTypeBinderDescriptor(Type TargetType, string Name)
    {
    }
}
