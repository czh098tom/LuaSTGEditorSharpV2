using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Exception;

namespace LuaSTGEditorSharpV2.Core
{
    public abstract class PackedDataProviderServiceBase<TData> : IPackedDataProviderService<TData>
        where TData : class
    {
        private class RegisteredDataProviderServiceHandle(PackedDataProviderServiceBase<TData> providerService, 
            string id, PackageInfo packageInfo, TData data) 
            : IDisposable
        {
            private readonly string _id = id;
            private readonly PackageInfo _packageInfo = packageInfo;
            private readonly TData _data = data;

            private readonly PackedDataProviderServiceBase<TData> _providerService = providerService;

            private bool _disposed = false;

            public void Dispose()
            {
                if (_disposed) return;
                _providerService.Unload(_id, _packageInfo, _data);
                _disposed = true;
            }
        }

        private readonly Dictionary<string, PriorityQueue<TData, PackageInfo>> _registered = [];
        private readonly Dictionary<TData, PackageInfo> _data2PackageInfo = [];

        public IDisposable Register(string id, PackageInfo packageInfo, TData data)
        {
            try
            {
                if (!_registered.ContainsKey(id))
                {
                    _registered.Add(id, new PriorityQueue<TData, PackageInfo>());
                }
                _registered[id].Enqueue(data, packageInfo);
                _data2PackageInfo.Add(data, packageInfo);
                OnRegistered(id, packageInfo, data);
                return new RegisteredDataProviderServiceHandle(this, id, packageInfo, data);
            }
            catch (ArgumentException e)
            {
                throw new DuplicatedIDException(id, e);
            }
        }

        public PackageInfo GetPackageInfo(TData data)
        {
            return _data2PackageInfo[data];
        }

        private void Unload(string id, PackageInfo packageInfo, TData data)
        {
            if (_registered.TryGetValue(id, out var pq))
            {
                List<TData> services = new(pq.Count);
                List<PackageInfo> packageInfos = new(pq.Count);
                while (pq.TryDequeue(out TData? s, out PackageInfo? pi))
                {
                    if (pi != packageInfo)
                    {
                        services.Add(s);
                        packageInfos.Add(pi);
                    }
                }
                for (int i = 0; i < services.Count; i++)
                {
                    pq.Enqueue(services[i], packageInfos[i]);
                }
            }
            _data2PackageInfo.Remove(data);
            OnUnloaded(id, packageInfo, data);
        }

        protected virtual void OnRegistered(string id, PackageInfo packageInfo, TData data) { }
        protected virtual void OnUnloaded(string id, PackageInfo packageInfo, TData data) { }

        internal protected TData? GetDataOfID(string id)
        {
            if (_registered.ContainsKey(id) && _registered[id].Count > 0)
            {
                return _registered[id].Peek();
            }
            return null;
        }

        internal protected IReadOnlyDictionary<string, TData> GetRegisteredAvailableData()
        {
            var dict = new Dictionary<string, TData>();
            foreach (var kvp in _registered)
            {
                dict.Add(kvp.Key, kvp.Value.Peek());
            }
            return dict;
        }
    }
}
