using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using System.Globalization;

namespace LuaSTGEditorSharpV2.Core.Services
{
    public class LocalizationService
    {
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
                _resourceManagers.Add(assembly,
                    new ResourceManager($"{assembly.GetName().Name}.{_resourceName}", assembly));
            }
            return _resourceManagers[assembly].GetString(key) ?? $"key: {key}";
        }

        public void SetUICulture(CultureInfo cultureInfo)
        {
            currentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
            _onCultureChanged?.Invoke(this, new CultureInfoChangedEventHandler(cultureInfo));
        }
    }
}
