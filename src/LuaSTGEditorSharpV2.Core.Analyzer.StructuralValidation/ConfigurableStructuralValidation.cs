using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Analyzer.StructuralValidation
{
    [Serializable]
    public class ConfigurableStructuralValidation(StructuralValidationServiceProvider nodeServiceProvider, IServiceProvider serviceProvider) : StructuralValidationServiceBase(nodeServiceProvider, serviceProvider)
    {
        [JsonProperty] public HashSet<string> RequireParent { get; private set; } = new();
        [JsonProperty] public HashSet<string> RequireAncestor { get; private set; } = new();
        [JsonProperty] public bool IsClassNode { get; private set; } = false;
        [JsonProperty] public bool IsLeafNode { get; private set; } = false;
        [JsonProperty] public bool CanSetInactivate { get; private set; } = true;
        [JsonProperty] public bool ShouldUniqueAmongSiblings { get; private set; } = false;

        public override bool IsLeaf() => IsLeafNode;
        public override bool IsInvisible() => false;

        public override bool CanPlaceAsChildOf(NodeData node, StructuralValidationContext context)
        {
            if (!base.CanPlaceAsChildOf(node, context)) return false;
            if (!ValidateParent(context)) return false;
            if (!ValidateAncestor(context)) return false;
            if (!ValidateClass(context)) return false;
            if (!ValidateUniquness(context, node)) return false;
            return true;
        }

        public override bool CanDeactivate(NodeData node)
        {
            return CanSetInactivate;
        }

        private bool ValidateParent(StructuralValidationContext context)
        {
            foreach (var ancestor in context.EnumerateFromNearest())
            {
                if (GetServiceOfNode(ancestor).IsLeaf()) return false;
                if (!GetServiceOfNode(ancestor).IsInvisible())
                {
                    if (RequireParent.Contains(ancestor.TypeUID))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        private bool ValidateAncestor(StructuralValidationContext context)
        {
            foreach (var ancestor in context.EnumerateFromNearest())
            {
                if (GetServiceOfNode(ancestor).IsLeaf()) return false;
                if (!GetServiceOfNode(ancestor).IsInvisible())
                {
                    if (RequireAncestor.Contains(ancestor.TypeUID))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool ValidateClass(StructuralValidationContext context)
        {
            if (IsClassNode)
            {
                foreach (var ancestor in context.EnumerateFromNearest())
                {
                    if (GetServiceOfNode(ancestor).IsLeaf()) return false;
                    if (!GetServiceOfNode(ancestor).IsInvisible()) return false;
                }
                return true;
            }
            return true;
        }

        private bool ValidateUniquness(StructuralValidationContext context, NodeData node)
        {
            if (ShouldUniqueAmongSiblings)
            {
                var parent = context.Peek();
                if (parent is not null)
                {
                    foreach(NodeData sibling in parent.PhysicalChildren)
                    {
                        if (sibling != node && sibling.TypeUID == TypeUID) return false;
                    }
                }
            }
            return true;
        }
    }
}
