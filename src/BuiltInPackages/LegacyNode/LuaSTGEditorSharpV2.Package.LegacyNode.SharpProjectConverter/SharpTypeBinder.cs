using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Serialization;

using LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter
{
    public class SharpTypeBinder : ISerializationBinder
    {
        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            throw new NotImplementedException();
        }

        public Type BindToType(string? assemblyName, string typeName)
        {
            if (typeName.EndsWith(".AttrItem") || typeName.EndsWith(".DependencyAttrItem"))
            {
                return typeof(SharpAttribute);
            }
            return typeof(SharpNode);
        }
    }
}
