using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.ViewModel;
using static LuaSTGEditorSharpV2.PropertyView.PropertyItemViewModelBase;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public class PropertyTabViewModel : ViewModelBase
    {
        public class ItemValueUpdatedEventArgs(int index,
            ValueUpdatedEventArgs args) : EventArgs
        {
            public int Index { get; private set; } = index;
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

        public event EventHandler<PropertyView.ItemValueUpdatedEventArgs>? OnItemValueUpdatedRaw;

        public PropertyTabViewModel()
        {
            Properties.CollectionChanged += GetHookItemEventsMarshallingHandler<PropertyItemViewModelBase>
                (vm => vm.OnValueUpdated += Item_OnValueUpdated);
            Properties.CollectionChanged += GetHookItemEventsMarshallingHandler<PropertyItemViewModelBase>
                (vm => vm.OnItemValueUpdatedRaw += Item_OnValueUpdated);
        }

        private void Item_OnValueUpdated(object? sender, ValueUpdatedEventArgs e)
        {
            if (sender is not PropertyItemViewModelBase vm) return;
            OnItemValueUpdated?.Invoke(this, new ItemValueUpdatedEventArgs(Properties.IndexOf(vm), e));
        }

        private void Item_OnValueUpdated(object? sender, PropertyView.ItemValueUpdatedEventArgs e)
        {
            OnItemValueUpdatedRaw?.Invoke(this, e);
        }
    }
}
