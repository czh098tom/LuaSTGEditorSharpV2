using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.DockingWindows;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.Toolbox.ViewModel;

namespace LuaSTGEditorSharpV2.ServiceInstanceProvider
{
    public class DockingButtonRegisterer : IServiceInstanceProvider<DockingWindowRibbonButtonDescriptor>
    {
        public IReadOnlyCollection<DockingWindowRibbonButtonDescriptor> GetServiceInstances(IServiceProvider serviceProvider)
        {
            var arr = new List<DockingWindowRibbonButtonDescriptor>
            {
                new("toolbox",
                    new Uri("pack://application:,,,/LuaSTGEditorSharpV2.Toolbox;component/Docking.xaml"),
                    "button", "general", typeof(ToolboxPageViewModel), serviceProvider),
                new("property",
                    new Uri("pack://application:,,,/LuaSTGEditorSharpV2.PropertyView;component/Docking.xaml"),
                    "button", "general", typeof(PropertyPageViewModel), serviceProvider)
            };

            return arr;
        }
    }
}
