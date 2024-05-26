using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;
using static LuaSTGEditorSharpV2.PropertyView.PropertyItemViewModelBase;
using static LuaSTGEditorSharpV2.PropertyView.PropertyTabViewModel;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public class PropertyPageViewModel : AnchorableViewModelBase
    {
        public ObservableCollection<PropertyTabViewModel> Tabs { get; private set; } = [];

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

        public override string I18NTitleKey => "panel_property_title";

        public PropertyPageViewModel()
        {
            Tabs.CollectionChanged += GetHookItemEventsMarshallingHandler<PropertyTabViewModel>
                (vm => vm.OnItemValueUpdated += Tab_OnItemValueUpdated);
            Tabs.CollectionChanged += GetHookItemEventsMarshallingHandler<PropertyTabViewModel>
                (vm => vm.OnItemValueUpdatedRaw += Tab_OnItemValueUpdated);
        }

        private void Tab_OnItemValueUpdated(object? sender, PropertyTabViewModel.ItemValueUpdatedEventArgs e)
        {
            if (sender is not PropertyTabViewModel vm) return;
            Tab_OnItemValueUpdatedImpl(Tabs, new ItemValueUpdatedEventArgs(Tabs.IndexOf(vm), e));
        }

        private void Tab_OnItemValueUpdated(object? sender, ItemValueUpdatedEventArgs e)
        {
            Tab_OnItemValueUpdatedImpl(null, e);
        }

        private void Tab_OnItemValueUpdatedImpl(IReadOnlyList<PropertyTabViewModel>? tabs, ItemValueUpdatedEventArgs e)
        {
            var propertyViewService = HostedApplicationHelper.GetService<PropertyViewServiceProvider>();
            var editResult = propertyViewService.GetCommandOfEditingNode(
                e.Args.Args.NodeData,
                e.Args.Args.LocalServiceParam, tabs, e.Index,
                e.Args.Index,
                e.Args.Args.NewValue);

            PublishCommand(editResult.Command, 
                e.Args.Args.LocalServiceParam.Source, 
                [e.Args.Args.NodeData], 
                editResult.ShouldRefreshView);
        }

        public override void HandleSelectedNodeChangedImpl(object o, SelectedNodeChangedEventArgs args)
        {
            var param = new LocalServiceParam(SourceDocument ?? DocumentModel.Empty);
            if (SourceNodes.Length == 1)
            {
                var list = HostedApplicationHelper.GetService<PropertyViewServiceProvider>()
                    .GetPropertyViewModelOfNode(SourceNodes[0], param);
                LoadProperties(list);
            }
            else
            {
                var list = HostedApplicationHelper.GetService<PropertyViewServiceProvider>()
                    .GetPropertyViewModelOfNode(NodeData.Empty, param);
                LoadProperties(list);
            }
        }

        private void LoadProperties(IReadOnlyList<PropertyTabViewModel> viewModels)
        {
            var index = SelectedIndex;
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
