using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building.ResourceGathering
{
    public class ResourceGatheringContext : NodeContextWithSettings<ResourceGatheringServiceSettings>
    {
        private Stack<string> _resourceGroup = new();

        public ResourceGatheringContext(LocalServiceParam localSettings, ResourceGatheringServiceSettings serviceSettings)
            : base(localSettings, serviceSettings)
        {
        }

        public void PushResourceGroup(string name)
        {
            _resourceGroup.Push(name);
        }

        public string PopResourceGroup()
        {
            return _resourceGroup.Pop();
        }

        public IEnumerable<string> EnumerateResourceGroups()
        {
            return _resourceGroup;
        }
    }
}
