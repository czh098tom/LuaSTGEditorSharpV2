using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

using LuaSTGEditorSharpV2.Package.LinqSTG.Windows;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.PropertyView.Configurable;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.PropertyView.Specialized.LinqSTGBlueprintPatternButton
{
    public class LinqSTGBlueprintPatternButtonViewModel
        : NamedPropertyItemViewModel<LinqSTGBlueprintPatternButtonItemTerm>
    {
        private readonly BoundProperty _valueProperty = new();

        public string Value
        {
            get => _valueProperty.Value;
            set => _valueProperty.Value = value;
        }

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

        public LinqSTGBlueprintPatternButtonViewModel()
        {
            OpenWindow = new RelayCommand(() =>
            {
                var window = new BlueprintPatternWindow
                {
                    NetworkJson = Value
                };
                window.ShowDialog();
                Value = window.NetworkJson ?? string.Empty;
            });
        }

        protected override void ConfigureViewModel(LinqSTGBlueprintPatternButtonItemTerm term)
        {
            base.ConfigureViewModel(term);
            ButtonCaption = term.ResolvedCaption;
        }

        protected override void ConfigureBinding(LinqSTGBlueprintPatternButtonItemTerm term)
        {
            Bind(term.Mapping).ToOne(_valueProperty);
        }
    }
}
