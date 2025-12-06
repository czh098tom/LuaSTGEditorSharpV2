using LuaSTGEditorSharpV2.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public static partial class CheckedCommand
    {
        public static class Property
        {
            public static CommandBase? Modify(EditorNode node, string? propertyName, string newValue)
            {
                if (string.IsNullOrEmpty(propertyName)) return null;
                if (node.Source.HasProperty(propertyName))
                {
                    return AtomicCommand.EditProperty(node, propertyName, newValue);
                }
                else
                {
                    return AtomicCommand.AddProperty(node, propertyName, newValue);
                }
            }

            public static CommandBase? ModifyMany(IEnumerable<EditorNode> nodes, string? propertyName, string newValue)
            {
                return Commands.FromEnumerable(
                    nodes.Select(n => Modify(n, propertyName, newValue))
                );
            }
        }
    }
}
