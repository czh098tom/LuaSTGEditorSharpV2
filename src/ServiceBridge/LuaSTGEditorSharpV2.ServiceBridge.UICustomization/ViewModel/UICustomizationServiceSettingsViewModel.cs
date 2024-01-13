using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.ServiceBridge.UICustomization.ViewModel
{
    [DisplayName("")]
    [SettingsDisplay("settings_title_uiCustomization", 16384)]
    public class UICustomizationServiceSettingsViewModel : BaseViewModel
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
    }
}
