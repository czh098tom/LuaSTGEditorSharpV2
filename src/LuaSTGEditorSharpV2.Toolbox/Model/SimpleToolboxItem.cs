using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Input;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Toolbox.ViewModel;

namespace LuaSTGEditorSharpV2.Toolbox.Model
{ 
    public class SimpleToolboxItem(IServiceProvider serviceProvider) : ToolboxItemModelBase(serviceProvider)
    {
        [JsonProperty] public NodeData[] NodeTemplate = [];

        public virtual NodeData[] CreateNode()
        {
            return JsonConvert.DeserializeObject<NodeData[]>(JsonConvert.SerializeObject(NodeTemplate)) ?? [];
        }

        public override ToolboxItemViewModel CreateViewModel()
        {
            var vm = base.CreateViewModel();
            vm.NodeCreating += (o, e) => e.CreatedData = CreateNode();
            return vm;
        }
    }
}
