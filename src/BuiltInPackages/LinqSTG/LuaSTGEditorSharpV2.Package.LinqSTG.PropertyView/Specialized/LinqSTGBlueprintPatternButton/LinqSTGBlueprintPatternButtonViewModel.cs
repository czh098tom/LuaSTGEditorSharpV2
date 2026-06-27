using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Package.LinqSTG.Windows;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.PropertyView.Specialized.LinqSTGBlueprintPatternButton
{
    public class LinqSTGBlueprintPatternButtonViewModel : BasicPropertyItemViewModel
    {
        private string _buttonCaption = string.Empty;
        public string ButtonCaption
        {
            get => _buttonCaption;
            set
            {
                _buttonCaption = value;
                RaisePropertyChanged();
            }
        }

        public ICommand OpenWindow { get; }

        public LinqSTGBlueprintPatternButtonViewModel(IReadOnlyList<EditorNode> editorNode, string? key,
            BatchEditStatus isBatchSame, LocalServiceParam localServiceParam,
            PropertyEditWizardProviderService propertyEditWizardProvider)
            : base(editorNode, key, isBatchSame, localServiceParam, propertyEditWizardProvider)
        {
            OpenWindow = new RelayCommand(() =>
            {
                new BlueprintPatternWindow().ShowDialog();
            });
        }
    }

    [Inject(ServiceLifetime.Singleton, typeof(IBasicPropertyItemViewModelFactory<LinqSTGBlueprintPatternButtonViewModel>))]
    public class LinqSTGBlueprintPatternButtonViewModelFactory(PropertyEditWizardProviderService propertyEditWizardProviderService)
        : IBasicPropertyItemViewModelFactory<LinqSTGBlueprintPatternButtonViewModel>
    {
        public LinqSTGBlueprintPatternButtonViewModel Create(IReadOnlyList<EditorNode> nodeData, string? key,
            BatchEditStatus isBatchSame, LocalServiceParam localServiceParam)
        {
            return new LinqSTGBlueprintPatternButtonViewModel(nodeData, key, isBatchSame, localServiceParam,
                propertyEditWizardProviderService);
        }
    }
}
