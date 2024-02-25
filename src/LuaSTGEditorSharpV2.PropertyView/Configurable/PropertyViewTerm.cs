using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    public record class PropertyViewTerm(
        NodePropertyCapture? Mapping,
        LocalizableString Caption,
        PropertyViewEditorType? Editor = null)
    {
        public PropertyItemViewModelBase GetViewModel(NodePropertyAccessToken token)
        {
            return new BasicPropertyItemViewModel()
            {
                Name = Caption.GetLocalized(),
                Value = Mapping?.Capture(token) ?? string.Empty,
                Type = Editor
            };
        }
    }
}
