using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.PropertyView.ViewModel
{
    public class PropertyTabWrapperItemViewModel : PropertyItemViewModelBase
    {
        private ObservableCollection<PropertyTabViewModel> _tabs = [];
        private readonly NotifyCollectionChangedEventHandler _tabsCollectionChangedHandler;
        private bool _disposedValue;

        public ObservableCollection<PropertyTabViewModel> Tabs
        {
            get => _tabs;
            set
            {
                _tabs = value;
                RaisePropertyChanged();
            }
        }

        public PropertyTabWrapperItemViewModel()
        {
            _tabsCollectionChangedHandler = GetHookItemEventsMarshallingHandler<PropertyTabViewModel>(
                HookTab);
        }

        public void Initialize(IReadOnlyList<PropertyTabViewModel> tabs,
            EditorNode editorNode, LocalServiceParam localServiceParam,
            PropertyEditWizardProviderService wizardProviderService)
        {
            base.Initialize([editorNode], localServiceParam, wizardProviderService);
            _tabs.CollectionChanged += _tabsCollectionChangedHandler;
            foreach (var tab in tabs)
            {
                _tabs.Add(tab);
            }
        }

        private void HookTab(PropertyTabViewModel tab)
        {
            tab.OnEdit += Tab_OnEdit;
        }

        private void DisposeTabs()
        {
            foreach (var tab in _tabs)
            {
                tab.OnEdit -= Tab_OnEdit;
                tab.Dispose();
            }
            _tabs.Clear();
        }

        private void Tab_OnEdit(object? sender, EditResult e)
        {
            RaiseOnEdit(e);
        }

        protected override void HandleEditorNodeOnPropertyChanged(object? sender, EditorNodePropertyChangedEventArgs e)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    DisposeTabs();
                    _tabs.CollectionChanged -= _tabsCollectionChangedHandler;
                }
                _disposedValue = true;
            }
            base.Dispose(disposing);
        }
    }
}
