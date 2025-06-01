using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.PropertyView.ViewModel
{
    public class BasicPropertyItemViewModel(NodeData nodeData, LocalServiceParam localServiceParam,
        string? key, EditorNodeFactory editorNodeFactory)
        : PropertyItemViewModelBase(nodeData, localServiceParam)
    {
        private string _name = string.Empty;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        public override EditResult ResolveEditingNodeCommand(NodeData nodeData, LocalServiceParam context, string edited)
        {
            return new EditResult(EditPropertyCommand.CreateEditCommandOnDemand(editorNodeFactory, nodeData, key, edited), LocalServiceParam);
        }
    }

    [Inject(ServiceLifetime.Singleton)]
    public class BasicPropertyItemViewModelFactory(EditorNodeFactory editorNodeFactory) 
        : IBasicPropertyItemViewModelFactory<BasicPropertyItemViewModel>
    {
        public BasicPropertyItemViewModel Create(NodeData nodeData, LocalServiceParam localServiceParam, string? key)
        {
            return new BasicPropertyItemViewModel(nodeData, localServiceParam, key, editorNodeFactory);
        }
    }
}
