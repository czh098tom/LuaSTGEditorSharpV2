using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public static class AtomicCommand
    {
        public static CommandBase AddNode(EditorNode parent, int pos, NodeData content) 
            => new AddChildCommand(parent, pos, content);

        public static CommandBase RemoveNode(EditorNode parent, int pos)
            => new RemoveChildCommand(parent, pos);

        public static CommandBase AddProperty(EditorNode node, string propName, string propValue)
            => new AddPropertyCommand(node, propName, propValue);

        public static CommandBase RemoveProperty(EditorNode node, string propName)
            => new RemovePropertyCommand(node, propName);

        public static CommandBase EditProperty(EditorNode node, string propName, string newValue)
            => new EditPropertyCommand(node, propName, newValue);
    }
}
