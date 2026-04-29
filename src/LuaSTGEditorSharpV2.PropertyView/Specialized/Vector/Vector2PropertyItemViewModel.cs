using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Parsing.Facade;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.PropertyView.Specialized.Vector
{
    public class Vector2PropertyItemViewModel(IReadOnlyList<EditorNode> nodeData, string? key, BatchEditStatus isBatchSame,
            LocalServiceParam localServiceParam, PropertyEditWizardProviderService propertyEditWizardProviderService)
        : BasicPropertyItemViewModel(nodeData, key, isBatchSame, localServiceParam, propertyEditWizardProviderService)
    {
        private string _x = string.Empty;
        public string X
        {
            get => _x;
            set
            {
                if (_x == value) return;
                _x = value;
                RaisePropertyChanged();
                if (!_isSyncing)
                {
                    Value = Vector2EditHelper.Compose(value, _y);
                }
            }
        }

        private string _y = string.Empty;
        public string Y
        {
            get => _y;
            set
            {
                if (_y == value) return;
                _y = value;
                RaisePropertyChanged();
                if (!_isSyncing)
                {
                    Value = Vector2EditHelper.Compose(_x, value);
                }
            }
        }

        public override string Value
        {
            get => base.Value;
            set
            {
                _isSyncing = true;
                base.Value = value;
                (_x, _y) = Vector2EditHelper.Decompose(value);
                _isSyncing = false;
            }
        }

        private bool _isSyncing = false;
    }

    [Inject(ServiceLifetime.Singleton, typeof(IBasicPropertyItemViewModelFactory<Vector2PropertyItemViewModel>))]
    public class Vector2PropertyItemViewModelFactory(
        PropertyEditWizardProviderService propertyEditWizardProviderService)
        : IBasicPropertyItemViewModelFactory<Vector2PropertyItemViewModel>
    {
        public Vector2PropertyItemViewModel Create(IReadOnlyList<EditorNode> nodeData, string? key, BatchEditStatus isBatchSame, LocalServiceParam localServiceParam)
        {
            return new Vector2PropertyItemViewModel(nodeData, key, isBatchSame, localServiceParam, propertyEditWizardProviderService);
        }
    }
}
