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

        public event EventHandler<EditResult>? OnEdit;

        public PropertyTabViewModel()
        {
            Properties.CollectionChanged += GetHookItemEventsMarshallingHandler<PropertyItemViewModelBase>
                (vm => vm.OnEdit += Item_OnEdit);
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
                    foreach (var item in Properties)
                    {
                        item.Dispose();
                    }
                    Properties.Clear();
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
