using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView.Configurable;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public class PropertyTabWrapperItemViewModel : PropertyItemViewModelBase
    {
        private ObservableCollection<PropertyTabViewModel> _tabs = [];

        public ObservableCollection<PropertyTabViewModel> Tabs
        {
            get => _tabs;
            set
            {
                _tabs = value;
                RaisePropertyChanged();
            }
        }

        public PropertyTabWrapperItemViewModel(IReadOnlyList<PropertyTabViewModel> tabs, NodeData nodeData, LocalServiceParam localServiceParam)
            : base(nodeData, localServiceParam)
        {
            _tabs.CollectionChanged += GetHookItemEventsMarshallingHandler<PropertyTabViewModel>(tab =>
            {
                tab.OnItemValueUpdatedRaw += Tab_OnItemValueUpdatedRaw;
                tab.OnItemValueUpdated += Tab_OnItemValueUpdated;
            });
            foreach (var tab in tabs)
            {
                _tabs.Add(tab);
            }
        }

        private void Tab_OnItemValueUpdated(object? sender, PropertyTabViewModel.ItemValueUpdatedEventArgs e)
        {
            RaiseOnItemValueUpdatedRawEvent(new ItemValueUpdatedEventArgs(0, e));
        }

        private void Tab_OnItemValueUpdatedRaw(object? sender, ItemValueUpdatedEventArgs e)
        {
            RaiseOnItemValueUpdatedRawEvent(e);
        }
    }
}
