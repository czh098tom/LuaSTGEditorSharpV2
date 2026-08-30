using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Parsing.Facade;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.PropertyView.Specialized.Vector
{
    public class Vector2PropertyItemViewModel : NamedPropertyItemViewModel<Vector2PropertyItemTerm>
    {
        public BoundProperty XProperty { get; } = new();

        public string X
        {
            get => XProperty.Value;
            set => XProperty.Value = value;
        }

        public BoundProperty YProperty { get; } = new();

        public string Y
        {
            get => YProperty.Value;
            set => YProperty.Value = value;
        }

        private NodePropertyCapture _capture = null!;

        public EditResult ApplyVector2Edit(string x, string y)
        {
            XProperty.SetValueWithoutPushingCommand(x);
            YProperty.SetValueWithoutPushingCommand(y);
            var value = Vector2EditHelper.Compose(x, y);
            var command = Commands.FromEnumerable(
                SourceNodes.Select(node => CheckedCommand.Property.Modify(
                    node.Document,
                    node.GetPath(),
                    _capture.Key,
                    value)));
            return new EditResult(command, LocalServiceParam);
        }

        protected override void ConfigureViewModel(Vector2PropertyItemTerm term)
        {
            base.ConfigureViewModel(term);
            ShowEditWindow = new RelayCommand(() =>
            {
                var result = WizardProviderService.GetEditResult(
                    Type?.Name ?? string.Empty,
                    this,
                    LocalServiceParam);
                if (result != null)
                {
                    RaiseOnEdit(result);
                }
            });
        }

        protected override void ConfigureBinding(Vector2PropertyItemTerm term)
        {
            _capture = term.Mapping;
            ForwardValueChanges(XProperty, nameof(X));
            ForwardValueChanges(YProperty, nameof(Y));
            Bind(term.Mapping).ToMany(
                (XProperty, YProperty),
                Vector2EditHelper.Compose,
                Vector2EditHelper.Decompose);
        }
    }
}
