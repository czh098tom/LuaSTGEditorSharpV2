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
        public EditorDocument Document { get; private set; }
        public NodePath ParentPath { get; private set; }
        public int Position { get; private set; }

        private NodeData? child;

        public RemoveChildCommand(EditorDocument document, NodePath parentPath, int position)
        {
            Document = document;
            ParentPath = parentPath;
            Position = position;
        }

        protected override void DoExecute(EditorDocument editorDocument)
        {
            var node = Document.RootEditorNode.GetNodeByPath(ParentPath) ?? throw new CommandExecutionException();
            child = node.RemoveAt(Position);
        }

        protected override void RevertExecution(EditorDocument editorDocument)
        {
            if (child == null) throw new InvalidOperationException("Command has not been executed yet.");
            var node = Document.RootEditorNode.GetNodeByPath(ParentPath) ?? throw new CommandExecutionException();
            node.Insert(Position, child);
        }
    }
}
