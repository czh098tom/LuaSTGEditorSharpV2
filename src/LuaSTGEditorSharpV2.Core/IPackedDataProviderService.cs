using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public interface IPackedDataProviderService<TData>
    {
        public IDisposable Register(string id, PackageInfo packageInfo, TData data);
        public PackageInfo GetPackageInfo(TData data);
    }
}
