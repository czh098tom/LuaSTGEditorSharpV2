using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Editor
{
    public class EditorNodeFactory(IServiceProvider serviceProvider)
    {
        private readonly Dictionary<NodeData, EditorNode> _node2Instances = [];
        private readonly Dictionary<IServiceProvider, EditorNode> _provider2Instances = [];

        public EditorNode GetOrCreate(NodeData source)
        {
            if (!_node2Instances.TryGetValue(source, out var node))
            {
                var scope = serviceProvider.CreateScope();
                node = new EditorNode(scope, source, this);
                _node2Instances.Add(source, node);
                _provider2Instances.Add(scope.ServiceProvider, node);
            }
            return node;
        }

        public EditorNode GetFromProvider(IServiceProvider serviceProvider)
        {
            return _provider2Instances[serviceProvider];
        }

        internal void Free(EditorNode editorNode)
        {
            _node2Instances.Remove(editorNode.Source);
            _provider2Instances.Remove(editorNode.Scope.ServiceProvider);
        }
    }
}
