using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public interface IPackedDataProviderService<TData>
    {
        public void Register(string id, PackageInfo packageInfo, TData data);
        public void UnloadPackage(PackageInfo packageInfo);
    }
}
