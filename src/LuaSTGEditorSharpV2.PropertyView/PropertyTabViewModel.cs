using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;
using static LuaSTGEditorSharpV2.PropertyView.PropertyItemViewModelBase;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public abstract class PropertyTabViewModel : ViewModelBase
    {
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
    }
}
