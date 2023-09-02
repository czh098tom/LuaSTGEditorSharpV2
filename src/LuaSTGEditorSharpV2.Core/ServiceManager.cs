using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using LuaSTGEditorSharpV2.Core.Exception;

namespace LuaSTGEditorSharpV2.Core
{
    public static class ServiceManager
    {
        private static readonly string _packageBasePath = "package";
        private static readonly string _manifestName = "manifest";
        private static readonly string _nodeDataBasePath = "Nodes";

        private static readonly JsonSerializerSettings _serviceDeserializationSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
        };

        private static readonly Dictionary<Type, ServiceInfo> _services2Info = new();
        private static readonly Dictionary<string, Type> _shortName2Services = new();

        public static Type GetServiceTypeOfShortName(string shortName)
        {
            return _shortName2Services[shortName];
        }

        public static void UseService(Type serviceType)
        {
            if (serviceType.BaseType?.GetGenericTypeDefinition() == typeof(NodeService<,,>))
            {
                string serviceName = serviceType.GetCustomAttribute<ServiceNameAttribute>()?.Name ?? serviceType.Name;
                string serviceShortName = serviceType.GetCustomAttribute<ServiceShortNameAttribute>()?.Name
                    ?? serviceType.Name;
                if (_shortName2Services.ContainsKey(serviceShortName))
                    throw new InvalidOperationException($"Service Short Name {serviceShortName} Duplicated.");

                Type[] genericArgs = serviceType.BaseType!.GetGenericArguments();
                Type self = genericArgs[0];
                Type context = genericArgs[1];
                Type settings = genericArgs[2];

                // find register func
                MethodInfo reg = serviceType.BaseType!.GetMethod(nameof(DefaultNodeService.Register)
                    , BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;
                Type regDelegateType = typeof(Action<,,>).MakeGenericType(typeof(string), typeof(PackageInfo)
                    , self);
                Delegate register = reg!.CreateDelegate(regDelegateType);

                // find reassign func
                MethodInfo rea = settings.BaseType!.GetMethod(nameof(DefaultServiceExtraSettings.Reassign)
                    , BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;
                Type reaDelegateType = typeof(Action<>).MakeGenericType(settings);
                Delegate reassign = rea!.CreateDelegate(reaDelegateType);

                _shortName2Services.Add(serviceShortName, serviceType);
                _services2Info.Add(serviceType, new ServiceInfo(serviceName, serviceShortName
                    , genericArgs[1], genericArgs[2]
                    , register, reassign));
            }
            else
            {
                throw new ArgumentException($"Argument {nameof(serviceType)} is not a NodeService.", nameof(serviceType));
            }
        }

        public static IReadOnlyList<Assembly> LoadPackage(string packageName)
        {
            var path = Process.GetCurrentProcess().MainModule?.FileName;
            return LoadPackageFromDirectory(Path.Combine(Path.GetDirectoryName(path)
                ?? throw new InvalidOperationException(), _packageBasePath, packageName));
        }

        public static IReadOnlyList<Assembly> LoadPackageFromDirectory(string basePath)
        {
            List<Assembly> assembly = new();
            PackageInfo packageInfo = LoadManifest(Path.Combine(basePath, _manifestName));
            if (!string.IsNullOrWhiteSpace(packageInfo.LibraryPath))
            {
                Assembly asm = Assembly.LoadFrom(Path.Combine(basePath, packageInfo.LibraryPath));
                assembly.Add(asm);
                foreach (var kvp in _services2Info)
                {
                    var serviceType = kvp.Key;
                    string serviceName = kvp.Value.Name;
                    string serviceAssembly = Path.Combine(basePath,
                        $"{Path.GetFileNameWithoutExtension(packageInfo.LibraryPath)}.{serviceName}.dll");
                    if (File.Exists(serviceAssembly))
                    {
                        asm = Assembly.LoadFrom(serviceAssembly);
                        assembly.Add(asm);
                    }
                }
            }
            object?[] param = new object?[3];
            param[1] = packageInfo;
            foreach (var kvp in _services2Info)
            {
                Type serviceType = kvp.Key;
                Delegate registerFunc = kvp.Value.RegisterFunction;
                string serviceShortName = kvp.Value.ShortName;
                foreach (var fileName in Directory.EnumerateFiles(Path.Combine(basePath, _nodeDataBasePath)
                    , $"*.{serviceShortName}", SearchOption.AllDirectories))
                {
                    string def;
                    using (FileStream fs = new(fileName, FileMode.Open, FileAccess.Read))
                    {
                        using StreamReader sr = new(fs);
                        def = sr.ReadToEnd();
                    }
                    object obj = JsonConvert.DeserializeObject(def, _serviceDeserializationSettings)
                        ?? throw new PackageLoadingException($"Failed to deserialize service defined at {fileName} .");
                    if (obj.GetType().IsAnyDerivedTypeOf(serviceType))
                    {
                        param[0] = serviceType.BaseType!.GetProperty(nameof(DefaultNodeService.TypeUID))!.GetValue(obj);
                        param[2] = obj;
                        registerFunc.DynamicInvoke(param);
                    }
                    else
                    {
                        throw new PackageLoadingException($"Deserialized object is not a service defined at {fileName} .");
                    }
                }
            }
            return assembly;
        }

        private static PackageInfo LoadManifest(string path)
        {
            string manifestString;
            try
            {
                using (FileStream fs = new(path, FileMode.Open, FileAccess.Read))
                {
                    using StreamReader sr = new(fs);
                    manifestString = sr.ReadToEnd();
                }
                return JsonConvert.DeserializeObject<PackageInfo>(manifestString)
                    ?? throw new PackageLoadingException($"Failed to deserialize package manifest at {path} .");
            }
            catch (System.Exception e)
            {
                throw new PackageLoadingException($"Failed to load package manifest at {path} .", e);
            }
        }

        public static void LoadLocalNodeService(LocalNodeServices services)
        {
            object?[] param = new object?[3];
            param[1] = services.PackageInfo;
            foreach (var tup in services.Services)
            {
                Type serviceType = _shortName2Services[tup.Item2];
                param[0] = serviceType.BaseType!.GetProperty(nameof(DefaultNodeService.TypeUID))!.GetValue(tup.Item1);
                var info = _services2Info[serviceType];
                param[2] = tup.Item1;
                info.RegisterFunction.DynamicInvoke(param);
            }
        }

        public static void ReplaceSettingsForService(Type serviceType, JObject settings)
        {
            var serviceInfo = _services2Info[serviceType];
            var settingsType = serviceInfo.SettingsType;
            var assign = serviceInfo.SettingsReplacementFunction;
            var settingsObj = JsonConvert.DeserializeObject(settings.ToString(), settingsType);
            assign.DynamicInvoke(settingsObj);
        }

        public static void ReplaceSettingsForServiceShortName(string serviceShortName, JObject settings)
        {
            ReplaceSettingsForService(_shortName2Services[serviceShortName], settings);
        }
    }
}
