using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Core.Editor;

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

        private EditorNode? editing = null;

        public PropertyPageViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Tabs.CollectionChanged += GetHookItemEventsMarshallingHandler<PropertyTabViewModel>
                (vm => vm.OnEdit += Tab_OnEdit);
        }

        private void Tab_OnEdit(object? sender, EditResult e)
        {
            PublishCommand(e.Command,
                e.LocalServiceParam.Source,
                SourceNodes,
                e.ShouldRefreshView);
        }

        public override void HandleSelectedNodeChangedImpl(object o, SelectedNodeChangedEventArgs args)
        {
            var param = new LocalServiceParam(SourceDocument ?? DocumentModel.Empty);
            if (SourceNodes.Length == 1)
            {
                editing = SourceNodes[0];
            }
            else
            {
                editing = null;
            }
            LoadNodeData(param, editing);
        }

        private void LoadNodeData(LocalServiceParam param, EditorNode? nodeData)
        {
            if (nodeData == null)
            {
                LoadProperties([]);
                return;
            }
            var list = ServiceProvider.GetRequiredService<PropertyViewServiceProvider>()
                .GetPropertyViewModelOfNode(nodeData, param);
            LoadProperties(list);
        }

        private void LoadProperties(IReadOnlyList<PropertyTabViewModel> viewModels)
        {
            var index = SelectedIndex;
            foreach (var tab in Tabs)
            {
                foreach (var item in tab.Properties)
                {
                    item.Dispose();
                }
            }
            Tabs.Clear();
            for (int i = 0; i < viewModels.Count; i++)
            {
                Tabs.Add(viewModels[i]);
            }
            if (index >= viewModels.Count)
            {
                SelectedIndex = viewModels.Count - 1;
            }
            if (index < 0)
            {
                SelectedIndex = 0;
            }
            else
            {
                SelectedIndex = index;
            }
        }
    }
}
