using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Command;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    public class ConfigurablePropertyViewService : PropertyViewServiceBase
    {
        [JsonProperty]
        public PropertyViewTerm[] Mapping { get; private set; } = Array.Empty<PropertyViewTerm>();

        protected override IReadOnlyList<PropertyViewModel> ResolvePropertyViewModelOfNode(NodeData nodeData, int subtype = 0)
        {
            List<PropertyViewModel> propertyViewModels = new(Mapping.Length);

            for (int i = 0; i < Mapping.Length; i++)
            {
                propertyViewModels.Add(new PropertyViewModel(
                    Mapping[i].LocalizedCaption.GetValueOrDefault(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
                        , Mapping[i].Caption)
                    , nodeData.Properties[Mapping[i].Mapping], Mapping[i].Type));
            }
            return propertyViewModels;
        }

        protected override CommandBase ResolveCommandOfEditingNode(NodeData nodeData
            , IReadOnlyList<PropertyViewModel> propertyList, int index, string edited, int subtype = 0)
        {
            return new EditPropertyCommand(nodeData, Mapping[index].Mapping, edited);
        }
    }
}
