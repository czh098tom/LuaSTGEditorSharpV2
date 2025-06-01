using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core.Model;
using System.Collections.ObjectModel;

namespace LuaSTGEditorSharpV2.Core.Editor
{
    public sealed class EditorNode : IDisposable
    {
        public IServiceProvider ServiceProvider => _scope.ServiceProvider;
        public NodeData Source { get; }
        public EditorDocument Document { get; }

        public event NotifyCollectionChangedEventHandler? OnChildrenChanged
        {
            add => _children.CollectionChanged += value;
            remove => _children.CollectionChanged -= value;
        }

        public IReadOnlyList<EditorNode> Children => _children;

        public event EventHandler<EditorNodePropertyAddedEventArgs>? OnPropertyAdded;
        public event EventHandler<EditorNodePropertyRemovedEventArgs>? OnPropertyRemoved;
        public event EventHandler<EditorNodePropertyChangedEventArgs>? OnPropertyChanged;

        private readonly IServiceScope _scope;
        private readonly ObservableCollection<EditorNode> _children = [];
        private readonly EditorNodeFactory _factory;

        private bool _disposed = false;

        internal EditorNode(IServiceScope scope, NodeData source, EditorDocument document, EditorNodeFactory factory)
        {
            this._factory = factory;
            this._scope = scope;
            Source = source;
            Document = document;
            foreach (var en in CreateChildrenRecursive(source))
            {
                _children.Add(en);
            }
        }

        private IEnumerable<EditorNode> CreateChildrenRecursive(NodeData source)
        {
            foreach (var n in source.PhysicalChildren)
            {
                yield return _factory.GetOrCreate(n, Document);
            }
        }

        public void Add(NodeData node)
        {
            _children.Add(_factory.GetOrCreate(node, Document));
            Source.Add(node);
        }

        public void Insert(int position, NodeData node)
        {
            _children.Insert(position, _factory.GetOrCreate(node, Document));
            Source.Insert(position, node);
        }

        public NodeData RemoveAt(int position)
        {
            var n = _children[position];
            _children.RemoveAt(position);
            n.Dispose();
            return Source.Remove(position);
        }

        public void Replace(int position, NodeData node)
        {
            RemoveAt(position);
            Insert(position, node);
        }

        public void AddProperty(string key, string value)
        {
            if (Source.Properties.ContainsKey(key))
            {
                throw new ArgumentException($"Property '{key}' already exists.");
            }
            Source.Properties[key] = value;
            OnPropertyAdded?.Invoke(this, new EditorNodePropertyAddedEventArgs(key, value));
        }

        public void RemoveProperty(string key)
        {
            if (!Source.Properties.Remove(key))
            {
                throw new KeyNotFoundException($"Property '{key}' does not exist.");
            }
            OnPropertyRemoved?.Invoke(this, new EditorNodePropertyRemovedEventArgs(key));
        }

        public void ChangeProperty(string key, string value)
        {
            if (Source.Properties.TryGetValue(key, out string? oldValue))
            {
                Source.Properties[key] = value;
                OnPropertyChanged?.Invoke(this, new EditorNodePropertyChangedEventArgs(key, oldValue, value));
            }
            else
            {
                throw new KeyNotFoundException($"Property '{key}' does not exist.");
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (var node in _children)
                    {
                        node.Dispose();
                    }
                    _scope.Dispose();
                    _factory.Free(this);
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EditorNode()
        {
            Dispose(false);
        }
    }
}
