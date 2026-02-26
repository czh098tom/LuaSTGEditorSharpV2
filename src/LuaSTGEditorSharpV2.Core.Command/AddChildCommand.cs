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
        public NodeData Child { get; private set; }
        public EditorDocument Document { get; private set; }
        public NodePath ParentPath { get; private set; }
        public int Position { get; private set; }

        public AddChildCommand(EditorDocument document, NodePath parentPath, int position, NodeData child)
        {
            Document = document;
            ParentPath = parentPath;
            Position = position;
            Child = child.DeepClone();
        }

        protected override void DoExecute(EditorDocument editorDocument)
        {
            var parent = Document.RootEditorNode.GetNodeByPath(ParentPath) 
                ?? throw new CommandExecutionException();
            parent.Insert(Position, Child);
        }

        protected override void RevertExecution(EditorDocument editorDocument)
        {
            var parent = Document.RootEditorNode.GetNodeByPath(ParentPath)
                ?? throw new CommandExecutionException();
            parent.RemoveAt(Position);
        }
    }
}
