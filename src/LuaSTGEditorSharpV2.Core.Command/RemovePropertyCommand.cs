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
    public class RemovePropertyCommand : ConcreteCommand
    {
        public NodeData Node { get; private set; }
        public string PropertyName { get; private set; }

        string? _beforeEdit;

        public RemovePropertyCommand(EditorNodeFactory factory, NodeData node, string propertyName) 
            : base(factory)
        {
            Node = node;
            PropertyName = propertyName;
        }

        protected override void DoExecute(EditorDocument editorDocument)
        {
            _beforeEdit = Node.Properties[PropertyName];
            var node = EditorNodeFactory.GetOrCreate(Node, editorDocument);
            node.RemoveProperty(PropertyName);
            Node.Properties.Remove(PropertyName);
        }

        protected override void RevertExecution(EditorDocument editorDocument)
        {
            if (_beforeEdit == null) throw new InvalidOperationException("Command has not been executed yet.");
            var node = EditorNodeFactory.GetOrCreate(Node, editorDocument);
            node.AddProperty(PropertyName, _beforeEdit);
        }
    }
}
