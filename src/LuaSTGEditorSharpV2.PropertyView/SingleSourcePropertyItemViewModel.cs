using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView
{
    public abstract class SingleSourcePropertyItemViewModel : PropertyItemViewModelBase
    {
        public EditorNode SourceNode { get; private init; }

        public SingleSourcePropertyItemViewModel(EditorNode editorNode, LocalServiceParam localServiceParam, 
            PropertyEditWizardProviderService wizardProviderService) 
            : base([editorNode], BatchEditStatus.AllSame, localServiceParam, wizardProviderService)
        {
            SourceNode = editorNode;
        }

        public override EditResult ResolveBatchEditingNodeCommand(IReadOnlyList<EditorNode> nodeData, LocalServiceParam context, string edited)
        {
            return ResolveEditingNodeCommand(nodeData[0], context, edited);
        }

        public abstract EditResult ResolveEditingNodeCommand(EditorNode nodeData, LocalServiceParam context, string edited);
    }
}
