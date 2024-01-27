using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2
{
    public class EditingDocumentModel : IDocument
    {
        public DocumentModel Target { get; private set; }

        public CommandBuffer CommandBuffer { get; private set; } = new CommandBuffer();

        public EditingDocumentModel(DocumentModel target)
        {
            Target = target;
        }

        public string? FilePath => Target.FilePath;
        public NodeData Root => Target.Root;
        public bool IsOnDisk => Target.IsOnDisk;
        public string FileName => Target.FileName;

        public bool IsModified => CommandBuffer.CanUndo;

        public void Save()
        {
            Target.Save();
        }

        public void SaveAs(string filePath)
        {
            Target.SaveAs(filePath);
        }
    }
}
