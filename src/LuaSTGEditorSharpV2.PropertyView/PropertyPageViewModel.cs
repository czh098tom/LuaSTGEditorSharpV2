using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;
using static LuaSTGEditorSharpV2.PropertyView.PropertyTabViewModel;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public class PropertyPageViewModel : AnchorableViewModelBase
    {
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

        public override string I18NTitleKey => "panel_property_title";

        public PropertyPageViewModel()
        {
            Tabs.CollectionChanged += GetHookItemEventsMarshallingHandler<PropertyTabViewModel>
                (vm => vm.OnItemValueUpdated += Tab_OnItemValueUpdated);
        }

        private void Tab_OnItemValueUpdated(object? sender, ItemValueUpdatedEventArgs e)
        {
            if (sender is not PropertyTabViewModel vm) return;

            var propertyViewService = HostedApplicationHelper.GetService<PropertyViewServiceProvider>();
            var doc = SourceDocument;
            if (doc == null) return;
            var param = new LocalServiceParam(doc);
            if (SourceNodes.Length != 1) return;
            var editResult = propertyViewService.GetCommandOfEditingNode(
                SourceNodes[0],
                param, Tabs, Tabs.IndexOf(vm),
                vm.Properties.IndexOf(e.Item),
                e.Args.NewValue);

            PublishCommand(editResult.Command, doc, SourceNodes, editResult.ShouldRefreshView);
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
