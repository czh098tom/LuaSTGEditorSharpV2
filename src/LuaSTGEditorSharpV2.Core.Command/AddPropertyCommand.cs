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
        public EditorNode Node { get; private set; }
        public string PropertyName { get; private set; }
        public string Value { get; private set; }

        public AddPropertyCommand(EditorNodeFactory factory, EditorNode node, string propertyName, string value)
            : base(factory)
        {
            Node = node;
            PropertyName = propertyName;
            Value = value;
        }

        protected override void DoExecute(EditorDocument editorDocument)
        {
            var node = Node;
            node.AddProperty(PropertyName, Value);
        }

        protected override void RevertExecution(EditorDocument editorDocument)
        {
            var node = Node;
            node.RemoveProperty(PropertyName);
        }
    }
}
