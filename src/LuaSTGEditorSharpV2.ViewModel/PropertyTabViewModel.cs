using LuaSTGEditorSharpV2.Core.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static LuaSTGEditorSharpV2.ViewModel.PropertyItemViewModelBase;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class PropertyTabViewModel : ViewModelBase
    {
        public class ItemValueUpdatedEventArgs(PropertyItemViewModelBase item, 
            ValueUpdatedEventArgs args) : EventArgs
        {
            public PropertyItemViewModelBase Item { get; private set; } = item;
            public ValueUpdatedEventArgs Args { get; private set; } = args;
        }

        private string caption = string.Empty;

        public ObservableCollection<PropertyItemViewModelBase> Properties { get; private set; } = new();

        public string Caption
        {
            get => caption;
            set
            {
                caption = value;
                RaisePropertyChanged();
            }
        }

        public event EventHandler<ItemValueUpdatedEventArgs>? OnItemValueUpdated;

        public PropertyTabViewModel()
        {
            Properties.CollectionChanged += GetHookItemEventsMarshallingHandler<PropertyItemViewModelBase>
                (vm => vm.OnValueUpdated += Item_OnValueUpdated);
        }

        private void Item_OnValueUpdated(object? sender, ValueUpdatedEventArgs e)
        {
            if (sender is not PropertyItemViewModelBase vm) return;
            OnItemValueUpdated?.Invoke(this, new ItemValueUpdatedEventArgs(vm, e));
        }
    }
}
