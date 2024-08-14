using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

using Microsoft.Extensions.Logging;

using LuaSTGEditorSharpV2.Core.Exception;
using LuaSTGEditorSharpV2.Core.Settings;

namespace LuaSTGEditorSharpV2.Core
{
    public class NodePackageProvider(ILogger<NodePackageProvider> logger)
    {
        private static readonly string _packageBasePath = "package";
        private static readonly string _manifestName = "manifest";

        private static readonly JsonConverter _versionConverter = new VersionConverter();

        private static readonly JsonSerializerSettings _serviceDeserializationSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
        };

        private readonly ILogger<NodePackageProvider> _logger = logger;

        private readonly List<ServiceInfo> _infos = [];
        private readonly Dictionary<Type, ServiceInfo> _servicesProviderType2Info = [];
        private readonly Dictionary<Type, ServiceInfo> _servicesInstanceType2Info = [];
        private readonly Dictionary<string, ServiceInfo> _shortName2Info = [];

        public Type GetServiceTypeOfShortName(string shortName)
        {
            return _shortName2Info[shortName].ServiceInstanceType;
        }

        public void UseServiceProvider(Type serviceProviderType)
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

                var serviceInfo = new ServiceInfo()
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

        public PackageDescriptor LoadPackage(string packageName)
        {
            _logger.LogInformation("Begin loading package \"{package_name}\"", packageName);
            var path = Process.GetCurrentProcess().MainModule?.FileName;
            var descriptor = LoadPackageFromDirectory(Path.Combine(Path.GetDirectoryName(path)
                ?? throw new InvalidOperationException(), _packageBasePath, packageName));
            _logger.LogInformation("Loaded package \"{package_name}\"", packageName);
            return descriptor;
        }

        public PackageDescriptor LoadPackageFromDirectory(string basePath)
        {
            // Load Assembly first, to ensure all class dependended by text files loaded
            List<Assembly> assembly = [];
            List<IDisposable> serviceDisposeHandles = [];
            var manifestPath = Path.Combine(basePath, _manifestName);
            PackageManifest packageManifest = GetManifest(manifestPath);
            _logger.LogInformation("Loaded manifest from \"{path}\"", manifestPath);
            if (!string.IsNullOrWhiteSpace(packageManifest.LibraryPath))
            {
                var assemblyPath = Path.Combine(basePath, packageManifest.LibraryPath);
                Assembly asm = Assembly.LoadFrom(assemblyPath);
                assembly.Add(asm);
                _logger.LogInformation("Loaded main assembly from \"{path}\"", manifestPath);
                foreach (var kvp in _servicesProviderType2Info)
                {
                    string serviceName = kvp.Value.Name;
                    string serviceAssembly = Path.Combine(basePath,
                        $"{Path.GetFileNameWithoutExtension(packageManifest.LibraryPath)}.{serviceName}.dll");
                    if (File.Exists(serviceAssembly))
                    {
                        asm = Assembly.LoadFrom(serviceAssembly);
                        assembly.Add(asm);
                        _logger.LogInformation("Loaded service assembly from \"{path}\"", manifestPath);
                    }
                }
            }
            // Enumerate all infos and register all instances
            object?[] param = new object?[4];
            param[2] = new PackageInfo(packageManifest, basePath);
            foreach (var info in _infos)
            {
                Type serviceType = info.ServiceInstanceType;
                Type serviceProviderType = info.ServiceProviderType;
                Delegate registerFunc = info.RegisterFunction;
                param[0] = HostedApplicationHelper.GetService(serviceProviderType);
                string serviceShortName = info.ShortName;
                foreach (var fileName in Directory.EnumerateFiles(basePath
                    , $"*.{serviceShortName}", SearchOption.AllDirectories))
                {
                    string def;
                    using (FileStream fs = new(fileName, FileMode.Open, FileAccess.Read))
                    {
                        using StreamReader sr = new(fs);
                        def = sr.ReadToEnd();
                    }
                    object? instance;
                    try
                    {
                        instance = JsonConvert.DeserializeObject(def, serviceType, _serviceDeserializationSettings);
                        if (instance != null && instance.GetType().IsAnyDerivedTypeOf(serviceType))
                        {
                            RegisterImpl(serviceDisposeHandles, param, info, instance);
                            _logger.LogInformation("Loaded service instance for \"{type_uid}\" from \"{path}\"",
                                param[1], manifestPath);
                        }
                        else
                        {
                            throw new PackageLoadingException($"Deserialized object is not a service defined at {fileName} .");
                        }
                    }
                    catch (JsonException e)
                    {
                        _logger.LogException(e);
                        _logger.LogError("Parsing JSON from \"{file_name}\" failed.", fileName);
                    }
                }
                foreach (var asm in assembly)
                {
                    var instanceProviders = asm.GetTypes()
                        .Where(t => t.IsAnyDerivedTypeOf(info.ServiceInstanceProviderType))
                        .Select(t => Activator.CreateInstance(t) as IServiceInstanceProvider<object>);
                    foreach (var provider in instanceProviders ?? [])
                    {
                        foreach (var instance in provider?.GetServiceInstances() ?? [])
                        {
                            if (instance != null && instance.GetType().IsAnyDerivedTypeOf(serviceType))
                            {
                                RegisterImpl(serviceDisposeHandles, param, info, instance);
                                _logger.LogInformation("Loaded service instance for \"{type_uid}\" from \"{class}\"",
                                    param[1], provider);
                            }
                            else
                            {
                                throw new PackageLoadingException($"Deserialized object is not a service defined at {provider} .");
                            }
                        }
                    }
                }
            }
            return new PackageDescriptor(serviceDisposeHandles, assembly);
        }

        private PackageManifest GetManifest(string path)
        {
            string manifestString;
            try
            {
                using (FileStream fs = new(path, FileMode.Open, FileAccess.Read))
                {
                    using StreamReader sr = new(fs);
                    manifestString = sr.ReadToEnd();
                }
                return JsonConvert.DeserializeObject<PackageManifest>(manifestString, _versionConverter)
                    ?? throw new PackageLoadingException($"Failed to deserialize package manifest at {path} .");
            }
            catch (System.Exception e)
            {
                _logger.LogException(e);
                throw new PackageLoadingException($"Failed to load package manifest at {path} .", e);
            }
        }

        public void LoadLocalNodeService(LocalNodeServices services)
        {
            object?[] param = new object?[4];
            param[2] = services.PackageInfo;
            foreach (var tup in services.Services)
            {
                Type serviceProviderType = _shortName2Info[tup.Item2].ServiceProviderType;
                param[0] = HostedApplicationHelper.GetService(serviceProviderType);
                var info = _servicesProviderType2Info[serviceProviderType];
                param[1] = serviceProviderType.GetProperty(info.ServiceInstancePrimaryKeyName)!.GetValue(tup.Item1);
                param[3] = tup.Item1;
                info.RegisterFunction.DynamicInvoke(param);
            }
        }

        public void ReplaceSettingsForServiceIfValid(Type serviceProviderType, JObject settings)
        {
            if (_servicesProviderType2Info.TryGetValue(serviceProviderType, out var serviceInfo))
            {
                if (serviceInfo.HasSettings)
                {
                    var assign = serviceInfo.SettingsReplacementFunction;
                    var serviceProvider = HostedApplicationHelper.GetService(serviceProviderType) as ISettingsProvider;
                    if (serviceProvider?.Settings != null)
                    {
                        var settingsClone = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(serviceProvider.Settings),
                            serviceProvider.Settings.GetType());
                        if (settingsClone != null)
                        {
                            JsonConvert.PopulateObject(settings.ToString(), settingsClone);
                            assign.DynamicInvoke(HostedApplicationHelper.GetService(serviceProviderType), settingsClone);
                        }
                    }
                }
            }
        }

        public void ReplaceSettingsForServiceShortNameIfValid(string serviceShortName, JObject settings)
        {
            if (_shortName2Info.TryGetValue(serviceShortName, out var info))
            {
                ReplaceSettingsForServiceIfValid(info.ServiceProviderType, settings);
            }
        }

        public IReadOnlyDictionary<string, object> GetServiceShortName2SettingsDict()
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var info in _infos)
            {
                if (HostedApplicationHelper.GetService(info.ServiceProviderType) is ISettingsProvider isp)
                {
                    dictionary.Add(info.ShortName, isp.Settings);
                }
            }
            return dictionary;
        }

#pragma warning disable CA1822
        private void RegisterImpl(List<IDisposable> serviceDisposeHandles, object?[] param, ServiceInfo info, object? instance)
#pragma warning restore CA1822
        {
            param[1] = info.ServiceInstanceType.GetProperty(info.ServiceInstancePrimaryKeyName)!.GetValue(instance);
            param[3] = instance;
            if (info.RegisterFunction.DynamicInvoke(param) is IDisposable disposable)
            {
                serviceDisposeHandles.Add(disposable);
            }
        }
    }
}
