using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Editor.Extension;

namespace LuaSTGEditorSharpV2.ViewModel
{
    [Inject(ServiceLifetime.Scoped, key: ScopeKey.EditorNode)]
    public class NodeViewModel : ViewModelBase
    {
        public string Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                RaisePropertyChanged();
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                RaisePropertyChanged();
            }
        }

        public bool IsActivated
        {
            get => _isActivated;
            set
            {
                _isActivated = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<NodeViewModel> Children => _children;

        private string _icon = "";
        private string _text = "";
        private bool _isActivated = true;
        private readonly ObservableCollection<NodeViewModel> _children = [];
        private readonly ViewModelProviderServiceProvider _viewModelProviderServiceProvider;

        public EditorNode EditorNode { get; }
        public NodeData Source => EditorNode.Source;

        public NodeViewModel([FromKeyedServices(ScopeKey.EditorNode)] EditorNode source, 
            ViewModelProviderServiceProvider viewModelProviderServiceProvider)
        {
            EditorNode = source;
            this._viewModelProviderServiceProvider = viewModelProviderServiceProvider;
            foreach (var child in EditorNode.Children)
            {
                _children.Add(child.GetRequiredNodeService<NodeViewModel>());
            }
            source.OnChildrenChanged += HandleSourceChildrenChanged;
            source.OnPropertyAdded += (_, _) => UpdateViewModelRecursive();
            source.OnPropertyChanged += (_, _) => UpdateViewModelRecursive();
            source.OnPropertyRemoved += (_, _) => UpdateViewModelRecursive();

            UpdateViewModelRecursive();
        }

        private void HandleSourceChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                int i = e.NewStartingIndex;
                foreach (EditorNode en in e.NewItems!)
                {
                    _children.Insert(i, en.ServiceProvider.GetRequiredKeyedService<NodeViewModel>(ScopeKey.EditorNode));
                    i++;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (EditorNode en in e.OldItems!)
                {
                    _children.Remove(en.ServiceProvider.GetRequiredKeyedService<NodeViewModel>(ScopeKey.EditorNode));
                }
            }
        }

        private void UpdateViewModelRecursive()
        {
            _viewModelProviderServiceProvider.UpdateViewModelDataRecursive(this, new(EditorNode.Document));
        }
    }
}
