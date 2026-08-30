using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.PropertyView.Configurable;

namespace LuaSTGEditorSharpV2.PropertyView.ViewModel
{
    public class BasicPropertyItemViewModel : NamedPropertyItemViewModel<PropertyItemTerm>
    {
        public BoundProperty ValueProperty { get; } = new();

        public string Value
        {
            get => ValueProperty.Value;
            set => ValueProperty.Value = value;
        }

        public bool ValueConflicted => ValueProperty.HasConflict;

		protected override void ConfigureViewModel(PropertyItemTerm term)
        {
            base.ConfigureViewModel(term);
			ShowEditWindow = new RelayCommand(() =>
            {
                var result = WizardProviderService.GetEditResult(Type?.Name ?? string.Empty, this, LocalServiceParam);
                if (result != null)
                {
                    RaiseOnEdit(result);
                    RaisePropertyChanged(nameof(Name));
                    RaisePropertyChanged(nameof(Value));
                    RaisePropertyChanged(nameof(Enabled));
                    RaisePropertyChanged(nameof(Type));
                }
            });
        }

        protected override void ConfigureBinding(PropertyItemTerm term)
        {
            Bind(term.Mapping).ToOne(ValueProperty);
        }
    }
}
