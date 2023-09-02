using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public class ResourceGatheringContext : NodeContext<ResourceGatheringServiceSettings>
    {
        private Stack<string> _resourceGroup = new ();

        public ResourceGatheringContext(LocalSettings localSettings, ResourceGatheringServiceSettings serviceSettings) 
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
