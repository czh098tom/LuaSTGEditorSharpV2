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
    public class StructuralValidationServiceBase(StructuralValidationServiceProvider nodeServiceProvider, IServiceProvider serviceProvider)
        : CompactNodeService<StructuralValidationServiceProvider, StructuralValidationServiceBase, StructuralValidationContext, StructuralValidationServiceSettings>(nodeServiceProvider, serviceProvider)
    {
        public virtual bool IsInvisible() => true;
        public virtual bool IsLeaf() => false;

        public override sealed StructuralValidationContext GetEmptyContext(LocalServiceParam localParam
            , StructuralValidationServiceSettings settings)
        {
            return new StructuralValidationContext(ServiceProvider, localParam, settings);
        }

        public bool CanPlaceAsChildOf(NodeData node, LocalServiceParam localParam
            , StructuralValidationServiceSettings serviceSettings)
        {
            var ctx = GetContextOfNode(node, localParam, serviceSettings);
            return CanPlaceAsChildOf(node, ctx);
        }

        public virtual bool CanPlaceAsChildOf(NodeData node, StructuralValidationContext context)
        {
            return !GetServiceOfNode(node).IsLeaf();
        }

        public virtual bool CanDeactivate(NodeData node)
        {
            return true;
        }
    }
}
