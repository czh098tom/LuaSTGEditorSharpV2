using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public class RemovePropertyCommand : ConcreteCommand
    {
        public EditorDocument Document { get; private set; }
        public NodePath ParentPath { get; private set; }
        public string PropertyName { get; private set; }

        string? _beforeEdit;

        public RemovePropertyCommand(EditorDocument document, NodePath parentPath, string propertyName)
        {
            Document = document;
            ParentPath = parentPath;
            PropertyName = propertyName;
        }

        protected override void DoExecute(EditorDocument editorDocument)
        {
            var node = Document.RootEditorNode.GetNodeByPath(ParentPath) ?? throw new CommandExecutionException();
            _beforeEdit = node.Source.Properties[PropertyName];
            node.RemoveProperty(PropertyName);
            node.Source.Properties.Remove(PropertyName);
        }

        protected override void RevertExecution(EditorDocument editorDocument)
        {
            if (_beforeEdit == null) throw new InvalidOperationException("Command has not been executed yet.");
            var node = Document.RootEditorNode.GetNodeByPath(ParentPath) ?? throw new CommandExecutionException();
            node.AddProperty(PropertyName, _beforeEdit);
        }
    }
}
