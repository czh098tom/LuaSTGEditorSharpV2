using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public class PropertyTabViewModel : ViewModelBase, IDisposable
    {
        private string caption = string.Empty;

        public ObservableCollection<PropertyItemViewModelBase> Properties { get; private set; } = [];

        private readonly NotifyCollectionChangedEventHandler _propertiesCollectionChangedHandler;
        private bool disposedValue;

        public string Caption
        {
            get => caption;
            set
            {
                caption = value;
                RaisePropertyChanged();
            }
        }

        public bool AllowBatchEditing { get; }

        public event EventHandler<EditResult>? OnEdit;

        public PropertyTabViewModel(bool allowBatchEditing = false)
        {
            _propertiesCollectionChangedHandler =
                GetHookItemEventsMarshallingHandler<PropertyItemViewModelBase>(HookItem);
            Properties.CollectionChanged += _propertiesCollectionChangedHandler;
            AllowBatchEditing = allowBatchEditing;
        }

        private void HookItem(PropertyItemViewModelBase item)
        {
            item.OnEdit += Item_OnEdit;
        }

        private void DisposeItems()
        {
            foreach (var item in Properties)
            {
                item.OnEdit -= Item_OnEdit;
                item.Dispose();
            }
            Properties.Clear();
        }

        private void Item_OnEdit(object? sender, EditResult e)
        {
            OnEdit?.Invoke(this, e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposeItems();
                    Properties.CollectionChanged -= _propertiesCollectionChangedHandler;
                    OnEdit = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
