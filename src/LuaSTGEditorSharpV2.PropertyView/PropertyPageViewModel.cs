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
        public NodeData? SourceNode { get; private set; }

        public DocumentModel? SourceDocument { get; private set; }

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
            if (SourceNode == null) return;
            var command = propertyViewService.GetCommandOfEditingNode(
                SourceNode,
                param, Tabs, Tabs.IndexOf(vm),
                vm.Properties.IndexOf(e.Item),
                e.Args.NewValue);

            PublishCommand(command, doc, SourceNode);
        }

        public override void HandleSelectedNodeChanged(object o, SelectedNodeChangedEventArgs args)
        {
            var doc = args.DocumentModel;
            if (doc == null) return;
            SourceDocument = doc;
            var node = args.NodeData ?? NodeData.Empty;
            SourceNode = node;
            var param = new LocalServiceParam(doc);
            var list = HostedApplicationHelper.GetService<PropertyViewServiceProvider>().GetPropertyViewModelOfNode(node, param);
            LoadProperties(list);
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
