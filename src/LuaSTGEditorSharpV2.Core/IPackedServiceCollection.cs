using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public interface IPackedServiceCollection : IReadOnlyCollection<PackedServiceInfo>
    {
        public IReadOnlyDictionary<Type, PackedServiceInfo> ServicesProviderType2Info { get; }
        public IReadOnlyDictionary<Type, PackedServiceInfo> ServicesInstanceType2Info { get; }
        public IReadOnlyDictionary<string, PackedServiceInfo> ShortName2Info { get; }
    }
}
