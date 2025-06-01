using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Extensions.DependencyInjection;

using CommunityToolkit.Mvvm.Input;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.PropertyView.ViewModel
{
    public class CollectionCountPropertyItemViewModel : BasicPropertyItemViewModel
    {
        private readonly ICommand _increase;
        public ICommand Increase => _increase;

        private readonly ICommand _decrease;
        public ICommand Decrease => _decrease;

        public CollectionCountPropertyItemViewModel(NodeData nodeData, LocalServiceParam localServiceParam,
            string? key, EditorNodeFactory editorNodeFactory)
            : base(nodeData, localServiceParam, key, editorNodeFactory)
        {
            _increase = new RelayCommand(() =>
            {
                if (int.TryParse(Value, out var count))
                {
                    Value = (count + 1).ToString();
                }
            });
            _decrease = new RelayCommand(() =>
            {
                if (int.TryParse(Value, out var count))
                {
                    Value = (count - 1).ToString();
                }
            });
        }

        public override EditResult ResolveEditingNodeCommand(NodeData nodeData, LocalServiceParam context, string edited)
        {
            return base.ResolveEditingNodeCommand(nodeData, context, edited) with
            {
                ShouldRefreshView = true
            };
        }
    }

    [Inject(ServiceLifetime.Singleton)]
    public class CollectionCountPropertyItemViewModelFactory(EditorNodeFactory editorNodeFactory) 
        : IBasicPropertyItemViewModelFactory<CollectionCountPropertyItemViewModel>
    {
        public CollectionCountPropertyItemViewModel Create(NodeData nodeData, LocalServiceParam localServiceParam, string? key)
        {
            return new CollectionCountPropertyItemViewModel(nodeData, localServiceParam, key, editorNodeFactory);
        }
    }
}
