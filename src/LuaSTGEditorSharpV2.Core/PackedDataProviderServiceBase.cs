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
        private readonly Dictionary<string, PriorityQueue<TData, PackageInfo>> _registered = [];
        private readonly Dictionary<TData, PackageInfo> _data2PackageInfo = [];

        public void Register(string id, PackageInfo packageInfo, TData data)
        {
            try
            {
                if (!_registered.ContainsKey(id))
                {
                    _registered.Add(id, new PriorityQueue<TData, PackageInfo>());
                }
                _registered[id].Enqueue(data, packageInfo);
                _data2PackageInfo.Add(data, packageInfo);
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

        public void UnloadPackage(PackageInfo packageInfo)
        {
            List<TData> services = [];
            List<PackageInfo> packageInfos = [];
            foreach (var kvp in _registered)
            {
                var pq = kvp.Value;
                services.Clear();
                packageInfos.Clear();
                services.Capacity = services.Capacity < pq.Count ? pq.Count : services.Capacity;
                packageInfos.Capacity = packageInfos.Capacity < pq.Count ? pq.Count : packageInfos.Capacity;
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
            List<TData> toRemove = [];
            foreach (var kvp in _data2PackageInfo)
            {
                if (kvp.Value == packageInfo)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var data in toRemove)
            {
                _data2PackageInfo.Remove(data);
            }
        }

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
