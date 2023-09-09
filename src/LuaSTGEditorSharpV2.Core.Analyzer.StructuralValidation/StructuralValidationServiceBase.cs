using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Analyzer.StructuralValidation
{
    /// <summary>
    /// Provide functionality of making parent or children legitimacy validation from <see cref="NodeData"/>.
    /// </summary>
    [ServiceShortName("valid"), ServiceName("Analyzer.StructuralValidation")]
    public class StructuralValidationServiceBase
        : NodeService<StructuralValidationServiceBase, StructuralValidationContext, StructuralValidationServiceSettings>
    {
        private static readonly StructuralValidationServiceBase _defaultService = new();

        static StructuralValidationServiceBase()
        {
            _defaultServiceGetter = () => _defaultService;
        }

        public static IEnumerable<NodeData> GetInvalidPositions(NodeData root, LocalParams settings)
        {
            var ctx = GetContextOfNode(root, settings);
            return GetInvalidPositionsRecursive(root, ctx);
        }

        public static bool CanInsert(NodeData parent, NodeData toInsert, LocalParams settings)
        {
            var ctx = GetContextOfNode(parent, settings);
            ctx.Push(parent);
            return GetInvalidPositionsRecursive(toInsert, ctx).Any();
        }

        public static bool CanInactivateFor(NodeData node)
        {
            return GetServiceOfNode(node).CanInactivate(node);
        }

        private static IEnumerable<NodeData> GetInvalidPositionsRecursive(NodeData root
            , StructuralValidationContext context)
        {
            if (root.PhysicalParent != null && !GetServiceOfNode(root).CanPlaceAsChildOf(root.PhysicalParent, context))
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

        public override sealed StructuralValidationContext GetEmptyContext(LocalParams localSettings)
        {
            return new StructuralValidationContext(localSettings, ServiceSettings);
        }

        protected bool CanPlaceAsChildOf(NodeData node, LocalParams settings)
        {
            var ctx = GetContextOfNode(node, settings);
            return CanPlaceAsChildOf(node, ctx);
        }

        public virtual bool IsInvisible() => true;
        public virtual bool IsLeaf() => false;

        protected virtual bool CanPlaceAsChildOf(NodeData node, StructuralValidationContext context)
        {
            return !GetServiceOfNode(node).IsLeaf();
        }

        protected virtual bool CanInactivate(NodeData node)
        {
            return true;
        }
    }
}
