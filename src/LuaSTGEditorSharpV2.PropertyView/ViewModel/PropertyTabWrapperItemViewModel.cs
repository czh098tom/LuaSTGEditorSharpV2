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

namespace LuaSTGEditorSharpV2.PropertyView.ViewModel
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
                tab.OnEdit += Tab_OnEdit;
            });
            foreach (var tab in tabs)
            {
                _tabs.Add(tab);
            }
        }

        private void Tab_OnEdit(object? sender, EditResult e)
        {
            RaiseOnEdit(e);
        }

        public override EditResult ResolveEditingNodeCommand(NodeData nodeData, LocalServiceParam context, string edited)
        {
            return new EditResult(LocalServiceParam);
        }
    }
}
