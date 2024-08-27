using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using System.Globalization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.Core.Services
{
    [Inject(lifetime: ServiceLifetime.Singleton)]
    public class LocalizationService(ILogger<LocalizationService> logger)
    {
        ILogger<LocalizationService> _logger = logger;

        public class CultureInfoChangedEventHandler(CultureInfo cultureInfo) : EventArgs
        {
            public CultureInfo CultureInfo { get; private set; } = cultureInfo;
        }

        private readonly string _resourceName = "Resources.Localized";

        private readonly Dictionary<Assembly, ResourceManager> _resourceManagers = [];

        private CultureInfo currentCulture = CultureInfo.CurrentUICulture;

        private event EventHandler<CultureInfoChangedEventHandler>? _onCultureChanged;

        public event EventHandler<CultureInfoChangedEventHandler> OnCultureChanged
        {
            add
            {
                value?.Invoke(this, new CultureInfoChangedEventHandler(currentCulture));
                _onCultureChanged += value;
            }
            remove
            {
                _onCultureChanged -= value;
            }
        }

        public string GetString(string key, Assembly assembly)
        {
            if (!_resourceManagers.ContainsKey(assembly))
            {
                var typeName = $"{assembly.GetName().Name}.{_resourceName}";
                try
                {
                    _resourceManagers.Add(assembly,
                        new ResourceManager(typeName, assembly));
                }
                catch (System.Exception e)
                {
                    _logger.LogException(e);
                    _logger.LogError("Load resource from {typeName} failed.", typeName);
                    return $"key: {key}";
                }
            }
            try
            {
                return _resourceManagers[assembly].GetString(key) ?? $"key: {key}";
            }
            catch (System.Exception e)
            {
                _logger.LogException(e);
                _logger.LogError("Load resource from {assembly} with key {resource_key} failed.", assembly, key);
                return $"key: {key}";
            }
        }

        public void SetUICulture(CultureInfo cultureInfo)
        {
            currentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
            _onCultureChanged?.Invoke(this, new CultureInfoChangedEventHandler(cultureInfo));
        }
    }
}
