using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.DockingWindows;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.Toolbox.ViewModel;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.ServiceInstanceProvider
{
    public class DockingTemplateRegisterer : IServiceInstanceProvider<DockingWindowDescriptor>
    {
        public IReadOnlyCollection<DockingWindowDescriptor> GetServiceInstances(IServiceProvider serviceProvider)
        {
            var arr = new List<DockingWindowDescriptor>();

            AddDocument(serviceProvider, arr);
            AddToolbox(serviceProvider, arr);
            AddPropertyView(serviceProvider, arr);

            return arr;
        }

        private static void AddDocument(IServiceProvider serviceProvider, List<DockingWindowDescriptor> arr)
        {
            AddImpl<DocumentViewModel>("pack://application:,,,/LuaSTGEditorSharpV2.View;component/Docking.xaml",
                serviceProvider, arr);
        }

        private static void AddToolbox(IServiceProvider serviceProvider, List<DockingWindowDescriptor> arr)
        {
            AddImpl<ToolboxPageViewModel>("pack://application:,,,/LuaSTGEditorSharpV2.Toolbox;component/Docking.xaml",
                serviceProvider, arr);
        }

        private static void AddPropertyView(IServiceProvider serviceProvider, List<DockingWindowDescriptor> arr)
        {
            AddImpl<PropertyPageViewModel>("pack://application:,,,/LuaSTGEditorSharpV2.PropertyView;component/Docking.xaml",
                serviceProvider, arr);
        }

        private static void AddImpl<TViewModel>(string uri, IServiceProvider serviceProvider, List<DockingWindowDescriptor> arr)
        {
            var type = typeof(TViewModel);
            var key = type.Name;
            arr.Add(new DockingWindowDescriptor(type, new Uri(uri), key, serviceProvider));
        }
    }
}
