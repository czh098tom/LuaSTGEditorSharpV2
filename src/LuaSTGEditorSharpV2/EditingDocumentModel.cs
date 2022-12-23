using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2
{
    public class EditingDocumentModel : DocumentModel
    {
        public DocumentModel Target { get; private set; }

        public CommandBuffer CommandBuffer { get; private set; } = new CommandBuffer();

        public EditingDocumentModel(DocumentModel target)
        {
            Target = target;
        }

        public override string? FilePath => Target.FilePath;
        public override NodeData Root => Target.Root;
        public override bool IsOnDisk => Target.IsOnDisk;

        public bool IsModified => CommandBuffer.CanUndo;

        public override void SaveAs(string filePath)
        {
            Target.SaveAs(filePath);
        }
    }
}
