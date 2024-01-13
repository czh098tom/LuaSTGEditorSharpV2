using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Settings;

namespace LuaSTGEditorSharpV2.Core.Services
{
    public class SettingsService(ILogger<SettingsService> logger)
    {
        private static readonly List<SettingsDescriptor> _empty = [];

        private static readonly string appendDir = "LuaSTGEditorSharpV2\\Settings";

        private readonly ILogger<SettingsService> _logger = logger;

        private IReadOnlyList<SettingsDescriptor> _settingsDescriptors = _empty;
        public IReadOnlyList<SettingsDescriptor> SettingsDescriptors => _settingsDescriptors ?? _empty;

        public void LoadSettings()
        {
            string baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appendDir);
            var settingsProviders = HostedApplicationHelper.GetServices<ISettingsProvider>();
            List<SettingsDescriptor> settings = [];
            foreach (var provider in settingsProviders)
            {
                var desc = provider.CreateDescriptor();
                var providerType = desc.ServiceProviderType;
                var settingsType = desc.SettingsType;
                settings.Add(desc);
                var fileName = Path.Combine(baseDir, $"{providerType.Name}.json");
                try
                {
                    using var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    using var sr = new StreamReader(fs);
                    var str = sr.ReadToEnd();
                    object? s = JsonConvert.DeserializeObject(str, settingsType)
                        ?? throw new InvalidOperationException("Json serialized is null");
                    provider.Settings = s;
                }
                catch (System.Exception e)
                {
                    _logger.LogException(e);
                    _logger.LogError("Parsing JSON from \"{file_name}\" failed.", fileName);
                }
            }
            settings = [.. settings.OrderBy(x => x.SortingOrder)];
            _settingsDescriptors = settings;
        }

        public void SaveSettings()
        {
            string baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appendDir);
            Directory.CreateDirectory(baseDir);
            foreach (var desc in SettingsDescriptors)
            {
                var fileName = Path.Combine(baseDir, $"{desc.ServiceProviderType.Name}.json");
                try
                {
                    using var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
                    using var sw = new StreamWriter(fs);
                    sw.Write(JsonConvert.SerializeObject(desc.SettingsProvider.Settings));
                }
                catch (System.Exception e)
                {
                    _logger.LogException(e);
                    _logger.LogError("Writing JSON to \"{file_name}\" failed.", fileName);
                }
            }
        }
    }
}
