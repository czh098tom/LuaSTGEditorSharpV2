﻿using System;
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
    public class NodePackageProvider(ILogger<NodePackageProvider> logger, IPackedServiceCollection packedServiceInfos)
    {
        public static readonly string CORE_PACKAGE_NAME = "Core";

        private static readonly string _packageBasePath = "package";

        private static readonly JsonConverter _versionConverter = new VersionConverter();

        private static readonly JsonSerializerSettings _serviceDeserializationSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
        };

        private readonly ILogger<NodePackageProvider> _logger = logger;

        private readonly HashSet<string> _loadedPackageName = [];

        public PackageDescriptor LoadPackage(PackageAssemblyDescriptor desc)
        {
            var packageName = desc.Name;
            if (_loadedPackageName.Contains(packageName))
            {
                _logger.LogInformation("package \"{package_name}\", has already loaded, skip current loading.", packageName);
            }
            _logger.LogInformation("Begin loading package \"{package_name}\"", packageName);
            var path = Process.GetCurrentProcess().MainModule?.FileName;
            var descriptor = LoadPackageFromDirectory(desc);
            _logger.LogInformation("Loaded package \"{package_name}\"", packageName);
            _loadedPackageName.Add(packageName);
            return descriptor;
        }

        public PackageDescriptor LoadPackageFromDirectory(PackageAssemblyDescriptor desc)
        {
            var basePath = desc.BasePath;
            var packageManifest = desc.Manifest;
            List<IDisposable> serviceDisposeHandles = [];
            // Enumerate all infos and register all instances
            object?[] param = new object?[4];
            param[2] = new PackageInfo(packageManifest, basePath);
            foreach (var info in packedServiceInfos)
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
                                param[1], fileName);
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
                foreach (var asm in desc.Assemblies)
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
                                    param[1], provider!.GetType());
                            }
                            else
                            {
                                throw new PackageLoadingException($"Deserialized object is not a service defined at {serviceProviderType} .");
                            }
                        }
                    }
                }
            }
            return new PackageDescriptor(serviceDisposeHandles, desc.Assemblies);
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
                Type serviceProviderType = packedServiceInfos.ShortName2Info[tup.Item2].ServiceProviderType;
                param[0] = HostedApplicationHelper.GetService(serviceProviderType);
                var info = packedServiceInfos.ServicesProviderType2Info[serviceProviderType];
                param[1] = serviceProviderType.GetProperty(info.ServiceInstancePrimaryKeyName)!.GetValue(tup.Item1);
                param[3] = tup.Item1;
                info.RegisterFunction.DynamicInvoke(param);
            }
        }

        public void ReplaceSettingsForServiceIfValid(Type serviceProviderType, JObject settings)
        {
            if (packedServiceInfos.ServicesProviderType2Info.TryGetValue(serviceProviderType, out var serviceInfo))
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
            if (packedServiceInfos.ShortName2Info.TryGetValue(serviceShortName, out var info))
            {
                ReplaceSettingsForServiceIfValid(info.ServiceProviderType, settings);
            }
        }

        public IReadOnlyDictionary<string, object> GetServiceShortName2SettingsDict()
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var info in packedServiceInfos)
            {
                if (HostedApplicationHelper.GetService(info.ServiceProviderType) is ISettingsProvider isp)
                {
                    dictionary.Add(info.ShortName, isp.Settings);
                }
            }
            return dictionary;
        }

        public IReadOnlyCollection<IDisposable> Register<T>(IServiceInstanceProvider<T> instanceProvider)
        {
            List<IDisposable> disposables = [];
            object?[] param = new object?[4];
            // use default manifest
            var path = Process.GetCurrentProcess().MainModule?.FileName;
            var basePath = Path.Combine(Path.GetDirectoryName(path)
                ?? throw new InvalidOperationException(), _packageBasePath, CORE_PACKAGE_NAME);
            param[2] = new PackageInfo(PackageManifest.CORE, basePath);
            Type serviceInstanceType = typeof(T);
            var info = packedServiceInfos.ServicesInstanceType2Info[serviceInstanceType];
            Type serviceProviderType = info.ServiceProviderType;
            param[0] = HostedApplicationHelper.GetService(serviceProviderType);
            foreach (var instance in instanceProvider.GetServiceInstances())
            {
                if (instance != null && instance.GetType().IsAnyDerivedTypeOf(serviceInstanceType))
                {
                    RegisterImpl(disposables, param, info, instance);
                    _logger.LogInformation("Loaded service instance for \"{type_uid}\" from \"{class}\"",
                        param[1], instanceProvider.GetType());
                }
                else
                {
                    throw new PackageLoadingException($"Deserialized object is not a service defined at {serviceProviderType} .");
                }
            }
            return disposables;
        }

#pragma warning disable CA1822
        private void RegisterImpl(List<IDisposable> serviceDisposeHandles, object?[] param, PackedServiceInfo info, object? instance)
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
