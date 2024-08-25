using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Exception;
using LuaSTGEditorSharpV2.Core.Settings;

namespace LuaSTGEditorSharpV2.Core
{
    public class BootstrapLoaderNodePackageProvider(ILogger<BootstrapLoaderNodePackageProvider> logger,
        PackedServiceCollection packedServiceInfos) : ISettingsProvider
    {
        private static readonly string _packageBasePath = "package";
        private static readonly string _manifestName = "manifest";

        private static readonly JsonConverter _versionConverter = new VersionConverter();

        public BootstrapLoaderNodePackageProviderSettings Settings { get; set; } = new();
        object ISettingsProvider.Settings
        {
            get => Settings;
            set => Settings = (BootstrapLoaderNodePackageProviderSettings)value;
        }

        private readonly HashSet<string> _loadedPackageName = [];

        public EventHandler? OnRefreshSettings;

        public void RefreshSettings()
        {
            OnRefreshSettings?.Invoke(this, EventArgs.Empty);
        }

        public IReadOnlyCollection<PackageAssemblyDescriptor> LoadAssemblies()
        {
            return Settings.Packages.Select(LoadAssembly).Where(s => s != null).ToArray()!;
        }

        public PackageAssemblyDescriptor? LoadAssembly(string packageName)
        {
            if (_loadedPackageName.Contains(packageName))
            {
                logger.LogInformation("package \"{package_name}\", has already loaded, skip current loading.", packageName);
                return null;
            }
            logger.LogInformation("Begin loading package \"{package_name}\"", packageName);
            var path = Process.GetCurrentProcess().MainModule?.FileName;
            var descriptor = LoadAssemblyFromPath(packageName, Path.Combine(Path.GetDirectoryName(path)
                ?? throw new InvalidOperationException(), _packageBasePath, packageName));
            logger.LogInformation("Loaded package \"{package_name}\"", packageName);
            _loadedPackageName.Add(packageName);
            return descriptor;
        }

        private PackageAssemblyDescriptor LoadAssemblyFromPath(string packageName, string basePath)
        {
            List<Assembly> assemblies = [];
            var manifestPath = Path.Combine(basePath, _manifestName);
            PackageManifest packageManifest = GetManifest(manifestPath);
            logger.LogInformation("Loaded manifest from \"{path}\"", manifestPath);
            if (!string.IsNullOrWhiteSpace(packageManifest.LibraryPath))
            {
                var assemblyPath = Path.Combine(basePath, packageManifest.LibraryPath);
                Assembly asm = Assembly.LoadFrom(assemblyPath);
                assemblies.Add(asm);
                logger.LogInformation("Loaded main assembly from \"{path}\"", manifestPath);
                foreach (var info in packedServiceInfos)
                {
                    string serviceName = info.Name;
                    string serviceAssembly = Path.Combine(basePath,
                        $"{Path.GetFileNameWithoutExtension(packageManifest.LibraryPath)}.{serviceName}.dll");
                    if (File.Exists(serviceAssembly))
                    {
                        asm = Assembly.LoadFrom(serviceAssembly);
                        assemblies.Add(asm);
                        logger.LogInformation("Loaded service assembly from \"{path}\"", manifestPath);
                    }
                }
            }
            return new PackageAssemblyDescriptor(packageName, basePath, packageManifest, assemblies);
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
                logger.LogException(e);
                throw new PackageLoadingException($"Failed to load package manifest at {path} .", e);
            }
        }
    }
}
