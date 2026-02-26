using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public class AddPropertyCommand : ConcreteCommand
    {
        public EditorDocument Document { get; private set; }
        public NodePath ParentPath { get; private set; }
        public string PropertyName { get; private set; }
        public string Value { get; private set; }

        public AddPropertyCommand(EditorDocument document, NodePath parentPath, string propertyName, string value)
        {
            Document = document;
            ParentPath = parentPath;
            PropertyName = propertyName;
            Value = value;
        }

        protected override void DoExecute(EditorDocument editorDocument)
        {
            var node = Document.RootEditorNode.GetNodeByPath(ParentPath) ?? throw new CommandExecutionException();
            node.AddProperty(PropertyName, Value);
        }

        protected override void RevertExecution(EditorDocument editorDocument)
        {
            var node = Document.RootEditorNode.GetNodeByPath(ParentPath) ?? throw new CommandExecutionException();
            node.RemoveProperty(PropertyName);
        }
    }
}
