using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Editor
{
    public class EditorDocument : IDocument, IDisposable
    {
        public DocumentModel Target { get; private set; }
        public EditorNode RootEditorNode { get; }

        private readonly CommandBuffer _commandBuffer;

        private bool disposed;

        public EditorDocument(DocumentModel target, EditorNodeFactory editorNodeFactory)
        {
            Target = target;
            _commandBuffer = new(this);
            RootEditorNode = editorNodeFactory.GetOrCreate(target.Root, this);
        }

        public string? FilePath => Target.FilePath;
        public NodeData Root => Target.Root;
        public string FileName => Target.FileName;

        public bool IsModified => _commandBuffer.IsModified;

        public bool CanUndo => _commandBuffer.CanUndo;
        public bool CanRedo => _commandBuffer.CanRedo;

        public void Save()
        {
            Target.Save();
            _commandBuffer.Save();
        }

        public void SaveAs(string filePath)
        {
            Target.SaveAs(filePath);
            _commandBuffer.Save();
        }

        public void ExecuteCommand(CommandBase command)
        {
            _commandBuffer.Execute(command);
        }

        public void Undo()
        {
            _commandBuffer.Undo();
        }

        public void Redo()
        {
            _commandBuffer.Redo();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    RootEditorNode.Dispose();
                }

                disposed = true;
            }
        }

        private class LexicographicalListComparer<T> : IComparer<IReadOnlyList<T>> where T : IComparable<T>
        {
            public int Compare(IReadOnlyList<T>? x, IReadOnlyList<T>? y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                int minLength = Math.Min(x.Count, y.Count);
                for (int i = 0; i < minLength; i++)
                {
                    int cmp = x[i].CompareTo(y[i]);
                    if (cmp != 0)
                    {
                        return cmp;
                    }
                }
                return x.Count.CompareTo(y.Count);
            }
        }

        public IEnumerable<EditorNode> OrderByViewOrder(IEnumerable<EditorNode> source)
        {
            Dictionary<EditorNode, List<int>> paths = [];
            foreach (var node in source)
            {
                if (node.Document != this)
                {
                    throw new InvalidOperationException("All nodes must belong to the same document.");
                }
                paths.Add(node, node.GetPath());
            }
            return source.OrderBy(n => paths[n], new LexicographicalListComparer<int>());
        }

        ~EditorDocument()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
