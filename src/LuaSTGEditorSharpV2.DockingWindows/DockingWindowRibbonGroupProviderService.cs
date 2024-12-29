using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fluent;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.WPF.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.DockingWindows
{
    [PackedServiceProvider]
    public class DockingWindowRibbonGroupProviderService(IServiceProvider serviceProvider)
        : ResourceService<DockingWindowRibbonGroupDescriptor, RibbonGroupBox>(serviceProvider)
    {
        private readonly Lazy<Dictionary<string, (RibbonGroupBox? box, int priority)>> buttonDescriptors = new(() => []);

        public IEnumerable<RibbonGroupBox> GetRibbonGroups()
        {
            var dict = buttonDescriptors.Value;
            while (_operations.TryDequeue(out var op))
            {
                switch (op)
                {
                    case Operation.Assign assign:
                        var groupBox = assign.Parse();
                        if (groupBox != null)
                        {
                            groupBox.DataContext = new DockingWindowRibbonGroupViewModel();
                        }
                        dict[assign.Key] = (groupBox, assign.Desc.Priority);
                        break;
                    case Operation.Remove remove:
                        dict.Remove(remove.Key);
                        break;
                    default:
                        break;
                }
            }

            var result = dict.Values.OrderBy(p => p.priority)
                .Select(p => p.box)
                .OfType<RibbonGroupBox>()
                .ToList();

            foreach (var item in result)
            {
                item.Items.Clear();
            }

            var buttonService = ServiceProvider.GetRequiredService<DockingWindowRibbonButtonProviderService>();
            var buttonDesc = buttonService.GetButtons();
            foreach (var buttonGroups in buttonDesc)
            {
                if (dict.TryGetValue(buttonGroups.Key, out var group) && group.box is RibbonGroupBox g)
                {
                    foreach (var button in buttonGroups)
                    {
                        if (button is ButtonInstanceDescriptor b)
                        {
                            g.Items.Add(b.Button);
                            if (b.Button?.DataContext is DockingWindowRibbonButtonViewModel bvm
                                && g.DataContext is DockingWindowRibbonGroupViewModel gvm)
                            {
                                bvm.OnShift += gvm.RaiseOnShift;
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
