using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.ServiceBridge.Services;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.ServiceBridge.ViewModel
{
    public class SettingsDialogViewModel : InjectableViewModel
    {
        public List<SettingsPageViewModel> SettingsPages { get; private set; } = [];

        public ObservableCollection<string> SettingsTitles { get; private set; } = [];

        private int _selectedIndex = 0;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(SelectedItem));
            }
        }

        public ObservableCollection<object> SelectedItem
        {
            get
            {
                if (_selectedIndex < SettingsPages.Count)
                {
                    return SettingsPages[_selectedIndex].PageItems;
                }
                else
                {
                    return [];
                }
            }
        }

        public SettingsDialogViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            try
            {
                var disp = serviceProvider.GetRequiredService<SettingsDisplayService>();
                foreach (var page in disp.MapViewModel())
                {
                    SettingsTitles.Add(page.Title);
                    SettingsPages.Add(page);
                }
            }
            catch (Exception)
            {
                // in editor no services instantiated, ignore
                return;
            }
        }
    }
}
