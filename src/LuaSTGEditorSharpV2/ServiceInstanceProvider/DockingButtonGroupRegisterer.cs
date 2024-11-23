using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.DockingWindows;

namespace LuaSTGEditorSharpV2.ServiceInstanceProvider
{
    public class DockingButtonGroupRegisterer : IServiceInstanceProvider<DockingWindowRibbonGroupDescriptor>
    {
        public IReadOnlyCollection<DockingWindowRibbonGroupDescriptor> GetServiceInstances(IServiceProvider serviceProvider)
        {
            var arr = new List<DockingWindowRibbonGroupDescriptor>()
            {
                new("general",
                    new Uri("pack://application:,,,/LuaSTGEditorSharpV2;component/Docking.xaml"),
                    "general", 0, serviceProvider)
            };

            return arr;
        }
    }
}
