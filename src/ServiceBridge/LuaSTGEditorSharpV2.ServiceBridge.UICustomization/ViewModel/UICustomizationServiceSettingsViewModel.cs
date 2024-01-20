using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Media;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.WPF.Converter;

namespace LuaSTGEditorSharpV2.ServiceBridge.UICustomization.ViewModel
{
    [DisplayName("")]
    [SettingsDisplay("settings_title_uiCustomization", 16384)]
    public class UICustomizationServiceSettingsViewModel : ViewModelBase
    {
        [JsonProperty("uri")]
        private string _uri = string.Empty;
        public string Uri 
        {
            get => _uri;
            set
            {
                _uri = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty("inspector_font_size")]
        private double _inspectorFontSize;
        public double InspectorFontSize
        {
            get => _inspectorFontSize;
            set
            {
                _inspectorFontSize = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty("inspector_font_family"), JsonConverter(typeof(StringFontFamilyConverter))]
        private FontFamily _inspectorFontFamily = new();
        public FontFamily InspectorFontFamily
        {
            get => _inspectorFontFamily;
            set
            {
                _inspectorFontFamily = value;
                RaisePropertyChanged();
            }
        }
    }
}
