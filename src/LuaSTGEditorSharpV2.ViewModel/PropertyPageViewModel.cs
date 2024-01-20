using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;
using static LuaSTGEditorSharpV2.ViewModel.PropertyTabViewModel;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class PropertyPageViewModel : ViewModelBase
    {
        public class TabItemValueUpdatedEventArgs(PropertyTabViewModel tab,
            ItemValueUpdatedEventArgs args) : EventArgs
        {
            public PropertyTabViewModel Tab { get; private set; } = tab;
            public ItemValueUpdatedEventArgs Args { get; private set; } = args;
        }

        public NodeData? Source { get; private set; }

        public ObservableCollection<PropertyTabViewModel> Tabs { get; private set; } = new();

        private int _selectedIndex = 0;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged();
            }
        }

        public event EventHandler<TabItemValueUpdatedEventArgs>? OnTabItemValueUpdated;

        public PropertyPageViewModel()
        {
            Tabs.CollectionChanged += GetHookItemEventsMarshallingHandler<PropertyTabViewModel>
                (vm=> vm.OnItemValueUpdated += Tab_OnItemValueUpdated);
        }

        private void Tab_OnItemValueUpdated(object? sender, ItemValueUpdatedEventArgs e)
        {
            if (sender is not PropertyTabViewModel vm) return;
            OnTabItemValueUpdated?.Invoke(this, new TabItemValueUpdatedEventArgs(vm, e));
        }

        public void LoadProperties(IReadOnlyList<PropertyTabViewModel> viewModels, NodeData source)
        {
            var index = SelectedIndex;
            Source = source;
            Tabs.Clear();
            for (int i = 0; i < viewModels.Count; i++)
            {
                Tabs.Add(viewModels[i]);
            }
            if (index >= viewModels.Count)
            {
                SelectedIndex = viewModels.Count - 1;
            }
            else
            {
                SelectedIndex = index;
            }
        }
    }
}
