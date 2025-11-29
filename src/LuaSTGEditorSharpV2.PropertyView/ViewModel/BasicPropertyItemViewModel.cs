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

namespace LuaSTGEditorSharpV2.PropertyView.ViewModel
{
    public class BasicPropertyItemViewModel : PropertyItemViewModelBase
    {
        private readonly string? key;
        private readonly EditorNodeFactory editorNodeFactory;
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

        public BasicPropertyItemViewModel(EditorNode editorNode, LocalServiceParam localServiceParam,
            string? key, EditorNodeFactory editorNodeFactory, PropertyEditWizardProviderService propertyEditWizardProvider) : base(editorNode, localServiceParam, propertyEditWizardProvider)
        {
            this.key = key;
            this.editorNodeFactory = editorNodeFactory;

            ShowEditWindow = new RelayCommand(() =>
            {
                var result = propertyEditWizardProvider.GetEditResult(Type?.Name ?? string.Empty, this, localServiceParam);
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

        public override EditResult ResolveEditingNodeCommand(EditorNode nodeData, LocalServiceParam context, string edited)
        {
            return new EditResult(EditPropertyCommand.CreateEditCommandOnDemand(editorNodeFactory, nodeData, key, edited), false, LocalServiceParam);
        }

        protected override void HandleEditorNodeOnPropertyChanged(object? sender, EditorNodePropertyChangedEventArgs e)
        {
            if (e.Key == key)
            {
                SetValueWithoutPushingEditCommand(e.NewValue);
            }
        }
    }

    [Inject(ServiceLifetime.Singleton)]
    public class BasicPropertyItemViewModelFactory(EditorNodeFactory editorNodeFactory, 
        PropertyEditWizardProviderService propertyEditWizardProviderService) 
        : IBasicPropertyItemViewModelFactory<BasicPropertyItemViewModel>
    {
        public BasicPropertyItemViewModel Create(EditorNode nodeData, LocalServiceParam localServiceParam, string? key)
        {
            return new BasicPropertyItemViewModel(nodeData, localServiceParam, key, editorNodeFactory, propertyEditWizardProviderService);
        }
    }
}
