using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Extensions.DependencyInjection;

using Xceed.Wpf.AvalonDock.Layout.Serialization;

using LuaSTGEditorSharpV2.Core.Settings;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Services;

namespace LuaSTGEditorSharpV2.Services
{
    [Inject(ServiceLifetime.Singleton)]
    public class MainWindowLayoutService(SettingsService settingsService) : ISettingsProvider, ISettingsSavedOnClose
    {
        private MainWindowLayoutSettings _settings = new();

        public object Settings 
        {
            get => _settings;
            set
            {
                _settings = (value as MainWindowLayoutSettings) ?? _settings;
            }
        }

        public event EventHandler<LayoutSerializationCallbackEventArgs>? LayoutSerializationCallback;

        private readonly object _lock = new();

        private XmlLayoutSerializer? _serializer = null;
        public XmlLayoutSerializer? Serializer
        {
            get
            {
                lock (_lock)
                {
                    if (_serializer != null) return _serializer;
                    try
                    {
                        var windows = System.Windows.Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                        if (windows != null)
                        {
                            lock (_lock)
                            {
                                _serializer = new XmlLayoutSerializer(windows.dockingManager);
                                _serializer.LayoutSerializationCallback += LayoutSerializationCallback;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                    return _serializer;
                }
            }
        }

        public void RefreshSettings() 
        {
            var serializer = Serializer;
            if (serializer != null)
            {
                try
                {
                    using MemoryStream ms = new(Encoding.GetEncoding("UTF-8").GetBytes(_settings.LayoutXML));
                    serializer.Deserialize(ms);
                }
                catch (Exception) { }
            }
        }

        public void SaveSettings()
        {
            var serializer = Serializer;
            if (serializer != null)
            {
                try
                {
                    using MemoryStream ms = new();
                    serializer.Serialize(ms);
                    _settings.LayoutXML = Encoding.GetEncoding("UTF-8").GetString(ms.ToArray());
                    settingsService.SaveSettings(this);
                }
                catch (Exception) { }
            }
        }
    }
}
