using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2
{
    public class EditingDocumentModel : IDocument
    {
        public DocumentModel Target { get; private set; }

        private readonly CommandBuffer _commandBuffer = new();

        public EditingDocumentModel(DocumentModel target)
        {
            Target = target;
        }

        public string? FilePath => Target.FilePath;
        public NodeData Root => Target.Root;
        public string FileName => Target.FileName;

        public bool IsModified => _commandBuffer.IsModified;

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
            _commandBuffer.Execute(command, new LocalServiceParam(this));
        }

        public void Undo()
        {
            _commandBuffer.Undo(new LocalServiceParam(this));
        }

        public void Redo()
        {
            _commandBuffer.Redo(new LocalServiceParam(this));
        }
    }
}
