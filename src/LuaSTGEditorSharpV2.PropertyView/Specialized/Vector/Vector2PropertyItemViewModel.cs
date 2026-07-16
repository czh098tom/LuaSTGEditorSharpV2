using System;
using System.Collections.Generic;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Editor;
using LuaSTGEditorSharpV2.Core.Parsing.Facade;
using LuaSTGEditorSharpV2.PropertyView.Configurable;
using LuaSTGEditorSharpV2.PropertyView.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.PropertyView.Specialized.Vector
{
    public class Vector2PropertyItemViewModel : NamedPropertyItemViewModel<Vector2PropertyItemTerm>
    {
        private readonly BoundProperty _xProperty = new();

        public string X
        {
            get => _xProperty.Value;
            set => _xProperty.Value = value;
        }

        private readonly BoundProperty _yProperty = new();

        public string Y
        {
            get => _yProperty.Value;
            set => _yProperty.Value = value;
        }

        protected override void ConfigureBinding(Vector2PropertyItemTerm term)
        {
            Bind(term.Mapping).ToMany(
                (_xProperty, _yProperty),
                Vector2EditHelper.Compose,
                Vector2EditHelper.Decompose);
        }
    }
}
