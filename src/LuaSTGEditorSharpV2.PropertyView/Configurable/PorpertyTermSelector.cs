using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    public class PorpertyTermSelector : IPropertyViewTerm
    {
        [JsonProperty] public ChildPropertyTerm? Candidate { get; private set; }
        [JsonProperty] public IPropertyViewTerm? IfEmpty { get; private set; }

        public PropertyItemViewModelBase GetViewModel(NodeData nodeData, PropertyViewContext context)
        {
            if (Candidate == null) return CreateDefaultModel(nodeData, context);
            var model = Candidate.GetViewModelImpl(nodeData, context);
            if (model.Tabs.Count <= 0) return CreateDefaultModel(nodeData, context);
            return model;
        }

        public CommandBase? ResolveCommandOfEditingNode(NodeData nodeData, PropertyViewContext context, string edited)
        {
            if (Candidate == null) return CreateDefaultCommand(nodeData, context, edited);
            var model = Candidate.GetViewModelImpl(nodeData, context);
            if (model.Tabs.Count <= 0) return CreateDefaultCommand(nodeData, context, edited);
            return null;
        }

        private PropertyItemViewModelBase CreateDefaultModel(NodeData nodeData, PropertyViewContext context)
        {
            return IfEmpty?.GetViewModel(nodeData, context)
                ?? new PropertyItemViewModelBase(nodeData, context.LocalParam)
                {
                    Value = ""
                };
        }

        private CommandBase? CreateDefaultCommand(NodeData nodeData, PropertyViewContext context, string edited)
        {
            return IfEmpty?.ResolveCommandOfEditingNode(nodeData, context, edited);
        }
    }
}
