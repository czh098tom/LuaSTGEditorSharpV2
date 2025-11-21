using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public class EditPropertyCommand : ConcreteCommand
    {
        public static CommandBase? CreateEditCommandOnDemand(EditorNodeFactory factory, EditorNode node, string? propertyName, string afterEdit)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return null;
            }
            else
            {
                if (node.Source.HasProperty(propertyName))
                {
                    return new EditPropertyCommand(node, propertyName, afterEdit);
                }
                else
                {
                    return new AddPropertyCommand(node, propertyName, afterEdit);
                }
            }
        }

        public EditorNode Node { get; private set; }
        public string PropertyName { get; private set; }
        public string AfterEdit { get; private set; }

        string? _beforeEdit;

        public EditPropertyCommand(EditorNode node, string propertyName, string afterEdit)
        {
            Node = node;
            PropertyName = propertyName;
            AfterEdit = afterEdit;
        }

        protected override void DoExecute(EditorDocument editorDocument)
        {
            _beforeEdit = Node.Source.Properties[PropertyName];
            var node = Node;
            node.ChangeProperty(PropertyName, AfterEdit);
        }

        protected override void RevertExecution(EditorDocument editorDocument)
        {
            if (_beforeEdit == null) throw new InvalidOperationException("Command has not been executed yet.");
            var node = Node;
            node.ChangeProperty(PropertyName, _beforeEdit);
        }
    }
}
