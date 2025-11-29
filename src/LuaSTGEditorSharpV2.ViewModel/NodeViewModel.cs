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

using PropertyTools;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Editor.Extension;

namespace LuaSTGEditorSharpV2.ViewModel
{
    [Inject(ServiceLifetime.Scoped, key: ScopeKey.EditorNode)]
    public class NodeViewModel : ViewModelBase, IDragSource, IDropTarget
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

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<NodeViewModel> Children => _children;

        private string _icon = "";
        private string _text = "";
        private bool _isActivated = true;
        private bool _isExpanded = true;
        private readonly ObservableCollection<NodeViewModel> _children = [];
        private readonly ViewModelProviderServiceProvider _viewModelProviderServiceProvider;
        private readonly IDragDropHandler _nodeDragDropHandler;

        public EditorNode EditorNode { get; }
        public NodeData Source => EditorNode.Source;

        public bool IsDraggable => EditorNode.Source.PhysicalParent != null;

        public NodeViewModel([FromKeyedServices(ScopeKey.EditorNode)] EditorNode source,
            [FromKeyedServices(ScopeKey.EditorNode)] IDragDropHandler nodeDragDropHandler,
            ViewModelProviderServiceProvider viewModelProviderServiceProvider)
        {
            EditorNode = source;
            _viewModelProviderServiceProvider = viewModelProviderServiceProvider;
            _nodeDragDropHandler = nodeDragDropHandler;
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

        public void Detach()
        {
        }

        public bool CanDrop(IDragSource node, DropPosition dropPosition, DragDropEffect effect)
        {
            return node is NodeViewModel nvm
                && !EditorNode.GetAllAncestors().Any(n => n == nvm.EditorNode)
                && _nodeDragDropHandler.CanDrop(nvm.EditorNode, dropPosition switch
                {
                    DropPosition.Add => DropRelativePosition.Child,
                    DropPosition.InsertBefore => DropRelativePosition.Before,
                    DropPosition.InsertAfter => DropRelativePosition.After,
                    _ => throw new NotSupportedException(),
                },
                effect switch
                {
                    DragDropEffect.Move => DragDropOperation.Move,
                    DragDropEffect.Copy => DragDropOperation.Copy,
                    _ => DragDropOperation.Move,
                });
        }

        public void Drop(IEnumerable<IDragSource> items, DropPosition dropPosition, DragDropEffect effect, DragDropKeyStates initialKeyStates)
        {
            _nodeDragDropHandler.Drop(items
                .OfType<NodeViewModel>()
                .Select(nvm => nvm.EditorNode), dropPosition switch
                {
                    DropPosition.Add => DropRelativePosition.Child,
                    DropPosition.InsertBefore => DropRelativePosition.Before,
                    DropPosition.InsertAfter => DropRelativePosition.After,
                    _ => throw new NotSupportedException(),
                },
                effect switch
                {
                    DragDropEffect.Move => DragDropOperation.Move,
                    DragDropEffect.Copy => DragDropOperation.Copy,
                    _ => DragDropOperation.Move,
                });
        }
    }
}
