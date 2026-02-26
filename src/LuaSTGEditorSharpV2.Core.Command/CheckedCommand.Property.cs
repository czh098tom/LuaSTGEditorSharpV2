using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public static partial class CheckedCommand
    {
        public static class Property
        {
            public static CommandBase? Modify(EditorDocument document, NodePath path, string? propertyName, string newValue)
            {
                var node = document.RootEditorNode.GetNodeByPath(path) ?? throw new CommandExecutionException();
                if (string.IsNullOrEmpty(propertyName)) throw new CommandExecutionException();
                if (node.Source.HasProperty(propertyName))
                {
                    return AtomicCommand.EditProperty(document, path, propertyName, newValue);
                }
                else
                {
                    return AtomicCommand.AddProperty(document, path, propertyName, newValue);
                }
            }
        }
    }
}
