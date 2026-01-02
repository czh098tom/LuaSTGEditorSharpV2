using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public class RemoveChildCommand : ConcreteCommand
    {
        public EditorNode Parent { get; private set; }
        public int Position { get; private set; }

        private NodeData? child;

        public RemoveChildCommand(EditorNode parent, int position)
        {
            Parent = parent;
            Position = position;
        }

        protected override void DoExecute(EditorDocument editorDocument)
        {
            var node = Parent;
            child = node.RemoveAt(Position);
        }

        protected override void RevertExecution(EditorDocument editorDocument)
        {
            if (child == null) throw new InvalidOperationException("Command has not been executed yet.");
            var node = Parent;
            node.Insert(Position, child);
        }
    }
}
