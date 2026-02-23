using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.ServiceBridge.Execution.ViewModel
{
    [DisplayName("")]
    [SettingsDisplay("settings_title_execution", displayKey: "execution")]
    public class ExecutionConfigServiceSettingsViewModel : ViewModelBase
    {
        [JsonProperty("target_executable")]
        private string? _targetExecutable;
        public string TargetExecutable
        {
            get => _targetExecutable ?? string.Empty;
            set
            {
                _targetExecutable = value;
                RaisePropertyChanged();
            }
        }
    }
}
