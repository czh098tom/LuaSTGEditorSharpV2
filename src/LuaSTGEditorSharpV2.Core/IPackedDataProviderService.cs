using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public interface IPackedDataProviderService<TData> : IPackedDataProviderService
    {
        public IDisposable Register(string id, PackageInfo packageInfo, TData data);
        public PackageInfo GetPackageInfo(TData data);

        IDisposable IPackedDataProviderService.Register(string id, PackageInfo packageInfo, object data)
        {
            return Register(id, packageInfo, (TData)data);
        }

        PackageInfo IPackedDataProviderService.GetPackageInfo(object data)
        {
            return GetPackageInfo((TData)data);
        }
    }

    public interface IPackedDataProviderService
    {
        public IDisposable Register(string id, PackageInfo packageInfo, object data);
        public PackageInfo GetPackageInfo(object data);
    }
}
