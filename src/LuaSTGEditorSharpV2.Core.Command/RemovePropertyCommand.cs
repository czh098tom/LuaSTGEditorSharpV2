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
        public EditorNode Node { get; private set; }
        public string PropertyName { get; private set; }

        string? _beforeEdit;

        public RemovePropertyCommand(EditorNode node, string propertyName)
        {
            Node = node;
            PropertyName = propertyName;
        }

        protected override void DoExecute(EditorDocument editorDocument)
        {
            _beforeEdit = Node.Source.Properties[PropertyName];
            var node = Node;
            node.RemoveProperty(PropertyName);
            Node.Source.Properties.Remove(PropertyName);
        }

        protected override void RevertExecution(EditorDocument editorDocument)
        {
            if (_beforeEdit == null) throw new InvalidOperationException("Command has not been executed yet.");
            var node = Node;
            node.AddProperty(PropertyName, _beforeEdit);
        }
    }
}
