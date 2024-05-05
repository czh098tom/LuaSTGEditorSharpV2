using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.ServiceBridge.Building.ViewModel
{
    [DisplayName("")]
    [SettingsDisplay("settings_title_buildTaskFactory", displayKey: "build_task_factory")]
    public class BuildTaskFactoryServiceSettingsViewModel : ViewModelBase
    {
        [JsonProperty("target_dir")]
        private string? _targetDir;
        public string TargetDir
        {
            get => _targetDir ?? string.Empty;
            set
            {
                _targetDir = value;
                RaisePropertyChanged();
            }
        }
    }
}
