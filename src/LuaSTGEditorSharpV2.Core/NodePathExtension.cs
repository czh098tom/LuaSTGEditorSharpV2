using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core
{
    public static class NodePathExtension
    {
        public static NodePath GetPath(this NodeData node)
        {
            NodePath path = [];
            NodeData? current = node;
            while (current.PhysicalParent is not null)
            {
                var index = current.PhysicalParent.PhysicalChildren.FindIndex(current);
                path.Add(index);
                current = current.PhysicalParent;
            }
            path.Reverse();
            return path;
        }

        public static NodePath GetPath(this EditorNode node)
        {
            NodePath path = [];
            EditorNode? current = node;
            while (current.Parent is not null)
            {
                var index = current.Parent.Children.FindIndex(current);
                path.Add(index);
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }

        public static NodeData? GetNodeByPath(this NodeData root, NodePath path)
        {
            NodeData? current = root;
            foreach (var index in path)
            {
                if (index < 0 || index >= current.PhysicalChildren.Count)
                {
                    return null;
                }
                current = current.PhysicalChildren[index];
            }
            return current;
        }

        public static EditorNode? GetNodeByPath(this EditorNode root, NodePath path)
        {
            EditorNode? current = root;
            foreach (var index in path)
            {
                if (index < 0 || index >= current.Children.Count)
                {
                    return null;
                }
                current = current.Children[index];
            }
            return current;
        }
    }
}
