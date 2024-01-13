using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.ServiceBridge.UICustomization.ViewModel
{
    public class DefaultUIStyleViewModel : BaseViewModel
    {
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
