using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public interface IDragDropHandler
    {
        public void Detach();
        public bool CanDrop(EditorNode editorNode, DropRelativePosition position, DragDropOperation operation);
        public void Drop(IEnumerable<EditorNode> items, DropRelativePosition position, DragDropOperation operation);
    }
}
