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

        public static IEnumerable<NodeData> GetInvalidPositions(NodeData root, LocalServiceParam param)
            => GetInvalidPositions(root, param, ServiceSettings);

        public static IEnumerable<NodeData> GetInvalidPositions(NodeData root, LocalServiceParam param
            , StructuralValidationServiceSettings serviceSettings)
        {
            var ctx = GetContextOfNode(root, param, serviceSettings);
            return GetInvalidPositionsRecursive(root, ctx);
        }

        public static bool CanInsert(NodeData parent, NodeData toInsert, LocalServiceParam param)
            => CanInsert(parent, toInsert, param, ServiceSettings);

        public static bool CanInsert(NodeData parent, NodeData toInsert, LocalServiceParam param
            , StructuralValidationServiceSettings serviceSettings)
        {
            var ctx = GetContextOfNode(parent, param, serviceSettings);
            ctx.Push(parent);
            return GetInvalidPositionsRecursive(toInsert, ctx).Any();
        }

        public static bool CanInactivateFor(NodeData node)
        {
            return GetServiceOfNode(node).CanDeactivate(node);
        }

        private static IEnumerable<NodeData> GetInvalidPositionsRecursive(NodeData root
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

        public virtual bool IsInvisible() => true;
        public virtual bool IsLeaf() => false;

        public override sealed StructuralValidationContext GetEmptyContext(LocalServiceParam localParam
            , StructuralValidationServiceSettings settings)
        {
            return new StructuralValidationContext(localParam, settings);
        }

        protected bool CanPlaceAsChildOf(NodeData node, LocalServiceParam localParam)
        {
            return CanPlaceAsChildOf(node, localParam, ServiceSettings);
        }

        protected bool CanPlaceAsChildOf(NodeData node, LocalServiceParam localParam
            , StructuralValidationServiceSettings serviceSettings)
        {
            var ctx = GetContextOfNode(node, localParam, serviceSettings);
            return CanPlaceAsChildOf(node, ctx);
        }

        protected virtual bool CanPlaceAsChildOf(NodeData node, StructuralValidationContext context)
        {
            return !GetServiceOfNode(node).IsLeaf();
        }

        protected virtual bool CanDeactivate(NodeData node)
        {
            return true;
        }
    }
}
