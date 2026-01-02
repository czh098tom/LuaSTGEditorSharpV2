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

        public CollectionCountPropertyItemViewModel(IReadOnlyList<EditorNode> nodeData, string? key,
            BatchEditStatus isBatchSame, LocalServiceParam localServiceParam, PropertyEditWizardProviderService propertyEditWizardProviderService)
            : base(nodeData, key, isBatchSame, localServiceParam, propertyEditWizardProviderService)
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

        public override EditResult ResolveBatchEditingNodeCommand(IReadOnlyList<EditorNode> nodeData, LocalServiceParam context, string edited)
        {
            return base.ResolveBatchEditingNodeCommand(nodeData, context, edited) with
            {
                ShouldRefreshView = true
            };
        }
    }

    [Inject(ServiceLifetime.Singleton)]
    public class CollectionCountPropertyItemViewModelFactory(PropertyEditWizardProviderService propertyEditWizardProviderService)
        : IBasicPropertyItemViewModelFactory<CollectionCountPropertyItemViewModel>
    {
        public CollectionCountPropertyItemViewModel Create(IReadOnlyList<EditorNode> nodeData, string? key, BatchEditStatus isBatchSame, LocalServiceParam localServiceParam)
        {
            return new CollectionCountPropertyItemViewModel(nodeData, key, isBatchSame, localServiceParam, propertyEditWizardProviderService);
        }
    }
}
