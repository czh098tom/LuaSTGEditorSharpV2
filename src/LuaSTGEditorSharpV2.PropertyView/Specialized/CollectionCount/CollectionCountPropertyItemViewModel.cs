using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.PropertyView.Specialized.CollectionCount
{
    public class CollectionCountPropertyItemViewModel : BasicPropertyItemViewModel
    {
        private ICommand _increase = null!;
        public ICommand Increase => _increase;

        private ICommand _decrease = null!;
        public ICommand Decrease => _decrease;

        public override void Initialize(IReadOnlyList<PropertySource> sources,
            LocalServiceParam localServiceParam,
            PropertyEditWizardProviderService propertyEditWizardProviderService)
        {
            base.Initialize(sources, localServiceParam, propertyEditWizardProviderService);
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
    }
}
