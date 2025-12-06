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

        public BasicPropertyItemViewModel(IReadOnlyList<EditorNode> editorNode, string? key,
            BatchEditStatus isBatchSame, LocalServiceParam localServiceParam, PropertyEditWizardProviderService propertyEditWizardProvider) 
            : base(editorNode, isBatchSame, localServiceParam, propertyEditWizardProvider)
        {
            this.key = key;

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

        public override EditResult ResolveBatchEditingNodeCommand(IReadOnlyList<EditorNode> nodeData, LocalServiceParam context, string edited)
        {
            return new EditResult(CheckedCommand.Property.ModifyMany(nodeData, key, edited), false, LocalServiceParam);
        }

        protected override void HandleEditorNodeOnPropertyChanged(object? sender, EditorNodePropertyChangedEventArgs e)
        {
            if (e.Key == key)
            {
                if (SourceNodes.Count == 0)
                {
                    BatchEditStatus = BatchEditStatus.AllSame;
                    SetValueWithoutPushingEditCommand(e.NewValue);
                    return;
                }
                else if (SourceNodes.Count == 1)
                {
                    BatchEditStatus = BatchEditStatus.AllSame;
                    SetValueWithoutPushingEditCommand(e.NewValue);
                    return;
                }
                else
                {
                    var first = SourceNodes[0].Source.GetProperty(key);
                    if (SourceNodes.All(n => n.Source.GetProperty(key) == first))
                    {
                        BatchEditStatus = BatchEditStatus.AllSame;
                    }
                    else
                    {
                        BatchEditStatus = BatchEditStatus.SomeDifferent;
                    }
                    SetValueWithoutPushingEditCommand(e.NewValue);
                }
            }
        }
    }

    [Inject(ServiceLifetime.Singleton)]
    public class BasicPropertyItemViewModelFactory(PropertyEditWizardProviderService propertyEditWizardProviderService)
        : IBasicPropertyItemViewModelFactory<BasicPropertyItemViewModel>
    {
        public BasicPropertyItemViewModel Create(IReadOnlyList<EditorNode> nodeData, string? key, BatchEditStatus isBatchSame, LocalServiceParam localServiceParam)
        {
            return new BasicPropertyItemViewModel(nodeData, key, isBatchSame, localServiceParam, propertyEditWizardProviderService);
        }
    }
}
