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
    public class AddChildCommand : ConcreteCommand
    {
        public EditorNode Parent { get; private set; }
        public NodeData Child { get; private set; }
        public int Position { get; private set; }

        public AddChildCommand(EditorNode parent, int position, NodeData child)
        {
            Parent = parent;
            Child = child.DeepClone();
            Position = position;
        }

        protected override void DoExecute(EditorDocument editorDocument)
        {
            var parent = Parent;
            parent.Insert(Position, Child);
        }

        protected override void RevertExecution(EditorDocument editorDocument)
        {
            var parent = Parent;
            parent.RemoveAt(Position);
        }
    }
}
