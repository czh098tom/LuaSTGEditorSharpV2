using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Exception;

namespace LuaSTGEditorSharpV2.Core
{
    public class PackageManager
    {
        private static readonly string _packageBasePath = "package";
        private static readonly string _manifestName = "manifest";
        private static readonly string _nodeDataBasePath = "nodes";

        private static readonly JsonSerializerSettings _serviceDeserializationSettings = new ()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
        };

        private static readonly HashSet<Type> _services = new();

        public static void UseService(Type serviceType)
        {
            if (serviceType.BaseType?.GetGenericTypeDefinition() == typeof(NodeService<,>))
            {
                _services.Add(serviceType);
            }
            else
            {
                throw new ArgumentException($"Argument {nameof(serviceType)} is not a NodeService", nameof(serviceType));
            }
        }

        public static void LoadPackage(string basePath)
        {
            PackageInfo packageInfo = LoadManifest(Path.Combine(basePath, _manifestName));
            if(!string.IsNullOrWhiteSpace(packageInfo.LibraryPath))
            {
                Assembly.LoadFrom(Path.Combine(basePath,packageInfo.LibraryPath));
                foreach (Type serviceType in _services)
                {
                    string serviceName = serviceType.GetCustomAttribute<ServiceNameAttribute>()?.Name ?? serviceType.Name;
                    string serviceAssembly = Path.Combine(basePath, 
                        $"{Path.GetFileNameWithoutExtension(packageInfo.LibraryPath)}.{serviceName}.dll");
                    if (File.Exists(serviceAssembly))
                    {
                        Assembly.LoadFrom(serviceAssembly);
                    }
                }
            }
            object?[] param = new object?[3];
            param[1] = packageInfo;
            foreach (Type serviceType in _services)
            {
                string serviceShortName = serviceType.GetCustomAttribute<ServiceShortNameAttribute>()?.Name ?? serviceType.Name;
#pragma warning disable CS8602
#pragma warning disable CS8600
                MethodInfo reg = serviceType.BaseType.GetMethod("Register"
                    , BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                Type delegateType = typeof(Action<,,>).MakeGenericType(typeof(string), typeof(PackageInfo)
                    , reg.DeclaringType.GetGenericArguments()[0]);
                Delegate register = reg.CreateDelegate(delegateType);
#pragma warning restore CS8600
#pragma warning restore CS8602
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
                        param[0] = serviceType.BaseType.GetProperty("TypeUID")?.GetValue(obj);
                        param[2] = obj;
                        register.DynamicInvoke(param);
                    }
                    else
                    {
                        throw new PackageLoadingException($"Deserialized object is not a service defined at {fileName} .");
                    }
                }
            }
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
    }
}
