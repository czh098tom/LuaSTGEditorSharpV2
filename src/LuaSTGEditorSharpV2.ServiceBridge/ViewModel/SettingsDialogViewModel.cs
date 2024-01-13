using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.ServiceBridge.Services;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.ServiceBridge.ViewModel
{
    public class SettingsDialogViewModel : BaseViewModel
    {
        public ObservableCollection<SettingsPageViewModel> SettingsPages { get; private set; } = [];

        public void Init()
        {
            var disp = HostedApplicationHelper.GetService<SettingsDisplayService>();
            foreach (var page in disp.MapViewModel())
            {
                SettingsPages.Add(page);
            }
        }
    }
}
