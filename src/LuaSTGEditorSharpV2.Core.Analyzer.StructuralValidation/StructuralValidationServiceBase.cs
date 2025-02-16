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
        : ProviderCachedNodeServiceBase<StructuralValidationServiceProvider>(nodeServiceProvider, serviceProvider)
    {
        public virtual bool IsInvisible() => true;
        public virtual bool IsLeaf() => false;

        public virtual bool CanPlaceAsChildOf(NodeData node, StructuralValidationContext context)
        {
            return IsLeaf();
        }

        public virtual bool CanDeactivate(NodeData node)
        {
            return true;
        }
    }
}
