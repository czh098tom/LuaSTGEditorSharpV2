﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Analyzer.StructuralValidation
{
    [ServiceShortName("valid"), ServiceName("Analyzer.StructuralValidation")]
    public class StructuralValidationServiceProvider 
        : NodeServiceProvider<StructuralValidationServiceProvider, StructuralValidationServiceBase, StructuralValidationContext, StructuralValidationServiceSettings>
    {
        private static readonly StructuralValidationServiceBase _defaultService = new();

        protected override StructuralValidationServiceBase DefaultService => _defaultService;

        public IEnumerable<NodeData> GetInvalidPositions(NodeData root, LocalServiceParam param)
            => GetInvalidPositions(root, param, ServiceSettings);

        public IEnumerable<NodeData> GetInvalidPositions(NodeData root, LocalServiceParam param
            , StructuralValidationServiceSettings serviceSettings)
        {
            var ctx = GetContextOfNode(root, param, serviceSettings);
            return GetInvalidPositionsRecursive(root, ctx);
        }

        public bool CanInsert(NodeData parent, NodeData toInsert, LocalServiceParam param)
            => CanInsert(parent, toInsert, param, ServiceSettings);

        public bool CanInsert(NodeData parent, NodeData toInsert, LocalServiceParam param
            , StructuralValidationServiceSettings serviceSettings)
        {
            var ctx = GetContextOfNode(parent, param, serviceSettings);
            ctx.Push(parent);
            return GetInvalidPositionsRecursive(toInsert, ctx).Any();
        }

        public bool CanInactivateFor(NodeData node)
        {
            return GetServiceOfNode(node).CanDeactivate(node);
        }

        private IEnumerable<NodeData> GetInvalidPositionsRecursive(NodeData root
            , StructuralValidationContext context)
        {
            if (root.PhysicalParent != null
                && !GetServiceOfNode(root).CanPlaceAsChildOf(root.PhysicalParent, context))
                yield return root;
            context.Push(root);
            foreach (var child in root.PhysicalChildren)
            {
                foreach (var inv in GetInvalidPositionsRecursive(child, context))
                {
                    yield return inv;
                }
            }
            context.Pop();
        }
    }
}
