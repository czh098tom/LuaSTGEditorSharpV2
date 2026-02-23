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
        public static CommandBase AddNode(EditorDocument document, NodePath path, int pos, NodeData content) 
            => new AddChildCommand(document, path, pos, content);

        public static CommandBase RemoveNode(EditorDocument document, NodePath path, int pos)
            => new RemoveChildCommand(document, path, pos);

        public static CommandBase AddProperty(EditorDocument document, NodePath path, string propName, string propValue)
            => new AddPropertyCommand(document, path, propName, propValue);

        public static CommandBase RemoveProperty(EditorDocument document, NodePath path, string propName)
            => new RemovePropertyCommand(document, path, propName);

        public static CommandBase EditProperty(EditorDocument document, NodePath path, string propName, string newValue)
            => new EditPropertyCommand(document, path, propName, newValue);
    }
}
