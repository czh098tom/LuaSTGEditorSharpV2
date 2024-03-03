using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.PropertyView.Configurable
{
    public abstract class PropertyViewTabTermBase
    {
        [JsonProperty] public LocalizableString? Caption { get; private set; }

        public abstract PropertyTabViewModel GetPropertyTabViewModel(NodeData nodeData, PropertyViewContext context);

        public abstract EditResult ResolveCommandOfEditingNode(NodeData nodeData,
            PropertyViewContext context, int itemIndex, string edited);
    }
}
