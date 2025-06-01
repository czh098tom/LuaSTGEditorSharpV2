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
