using LuaSTGEditorSharpV2.Core.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    public class PackedServiceCollection : IPackedServiceCollection
    {
        private readonly List<PackedServiceInfo> _infos = [];
        private readonly Dictionary<Type, PackedServiceInfo> _servicesProviderType2Info = [];
        private readonly Dictionary<Type, PackedServiceInfo> _servicesInstanceType2Info = [];
        private readonly Dictionary<string, PackedServiceInfo> _shortName2Info = [];

        public IReadOnlyDictionary<Type, PackedServiceInfo> ServicesProviderType2Info => _servicesInstanceType2Info;
        public IReadOnlyDictionary<Type, PackedServiceInfo> ServicesInstanceType2Info => _servicesInstanceType2Info;
        public IReadOnlyDictionary<string, PackedServiceInfo> ShortName2Info => _shortName2Info;

        public void Add<T>() where T : IPackedDataProviderService
        {
            Add(typeof(T));
        }

        public void Add(Type serviceProviderType)
        {
            Type? baseCoordType = serviceProviderType.BaseTypes()
                .FirstOrDefault(t => t.IsConstructedGenericType
                && t.GetGenericTypeDefinition() == typeof(PackedDataProviderServiceBase<>));
            if (baseCoordType != null)
            {
                string serviceName = serviceProviderType.GetCustomAttribute<ServiceNameAttribute>()?.Name ?? serviceProviderType.Name;
                string serviceShortName = serviceProviderType.GetCustomAttribute<ServiceShortNameAttribute>()?.Name
                    ?? serviceProviderType.Name;
                if (_shortName2Info.ContainsKey(serviceShortName))
                    throw new InvalidOperationException($"Service Short Name {serviceShortName} Duplicated.");

                Type providerType = serviceProviderType;
                var settingsProviderInterface = serviceProviderType.GetInterfaces()
                    .FirstOrDefault(t => t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(ISettingsProvider<>));
                Type? settingsType = settingsProviderInterface?.GetGenericArguments()?.GetOrDefault(0);
                Type instanceType = baseCoordType.GetGenericArguments()[0];
                string serviceInstancePrimaryKey = instanceType.GetCustomAttribute<PackagePrimaryKeyAttribute>()
                    ?.KeyPropertyName ?? throw new InvalidOperationException($"Cannot find service primary key.");

                // find register func
                MethodInfo reg = serviceProviderType.GetMethod(nameof(DefaultNodeServiceProvider.Register)
                    , BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;
                Type regDelegateType = typeof(Func<,,,,>).MakeGenericType(providerType, typeof(string), typeof(PackageInfo)
                    , instanceType, typeof(IDisposable));
                Delegate register = reg!.CreateDelegate(regDelegateType);

                // find reassign func
                Delegate? reassign = null;
                if (settingsType != null)
                {
                    MethodInfo? rea = serviceProviderType.GetMethod(nameof(DefaultNodeServiceProvider.LoadSettings)
                        , BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    Type reaDelegateType = typeof(Action<,>).MakeGenericType(providerType, settingsType);
                    reassign = rea?.CreateDelegate(reaDelegateType);
                }

                Type instanceProviderType = typeof(IServiceInstanceProvider<>).MakeGenericType(instanceType);

                var serviceInfo = new PackedServiceInfo()
                {
                    Name = serviceName,
                    ShortName = serviceShortName,
                    ServiceProviderType = providerType,
                    ServiceInstanceType = instanceType,
                    ServiceInstanceProviderType = instanceProviderType,
                    RegisterFunction = register,
                    ServiceInstancePrimaryKeyName = serviceInstancePrimaryKey,

                    SettingsType = settingsType,
                    SettingsReplacementFunction = reassign
                };

                _infos.Add(serviceInfo);
                _shortName2Info.Add(serviceShortName, serviceInfo);
                _servicesProviderType2Info.Add(serviceProviderType, serviceInfo);
                _servicesInstanceType2Info.Add(instanceType, serviceInfo);
            }
            else
            {
                throw new ArgumentException($"Argument {nameof(serviceProviderType)} is not a NodeServiceProvider.", nameof(serviceProviderType));
            }
        }

        public int Count => ((IReadOnlyCollection<PackedServiceInfo>)_infos).Count;

        public IEnumerator<PackedServiceInfo> GetEnumerator()
        {
            return ((IEnumerable<PackedServiceInfo>)_infos).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_infos).GetEnumerator();
        }
    }
}
