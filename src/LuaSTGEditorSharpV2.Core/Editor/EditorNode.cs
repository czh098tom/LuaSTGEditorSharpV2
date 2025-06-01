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
        public IServiceScope Scope { get; }
        public NodeData Source { get; }

        public event NotifyCollectionChangedEventHandler? OnChildrenChanged
        {
            add => children.CollectionChanged += value;
            remove => children.CollectionChanged -= value;
        }

        public event EventHandler<EditorNodePropertyAddedEventArgs>? OnPropertyAdded;
        public event EventHandler<EditorNodePropertyRemovedEventArgs>? OnPropertyRemoved;
        public event EventHandler<EditorNodePropertyChangedEventArgs>? OnPropertyChanged;

        private readonly ObservableCollection<EditorNode> children = [];
        private readonly EditorNodeFactory factory;

        private bool disposed = false;

        internal EditorNode(IServiceScope scope, NodeData source, EditorNodeFactory factory)
        {
            this.factory = factory;
            Scope = scope;
            Source = source;

            foreach (var en in CreateChildrenRecursive(source))
            {
                children.Add(en);
            }
        }

        private IEnumerable<EditorNode> CreateChildrenRecursive(NodeData source)
        {
            foreach (var n in source.PhysicalChildren)
            {
                yield return factory.GetOrCreate(n);
            }
        }

        public void Add(NodeData node)
        {
            children.Add(factory.GetOrCreate(node));
            Source.Add(node);
        }

        public void Insert(int position, NodeData node)
        {
            children.Insert(position, factory.GetOrCreate(node));
            Source.Insert(position, node);
        }

        public NodeData Remove(int position)
        {
            var n = children[position];
            children.RemoveAt(position);
            n.Dispose();
            return Source.Remove(position);
        }

        public void Replace(int position, NodeData node)
        {
            Remove(position);
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
            if (!disposed)
            {
                if (disposing)
                {
                    foreach (var node in children)
                    {
                        node.Dispose();
                    }
                    Scope.Dispose();
                    factory.Free(this);
                }
                disposed = true;
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
