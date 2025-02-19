using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fluent;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.WPF.Services;

namespace LuaSTGEditorSharpV2.DockingWindows
{
    [PackedServiceProvider]
    public class DockingWindowRibbonButtonProviderService(IServiceProvider serviceProvider)
        : ResourceService<DockingWindowRibbonButtonDescriptor, Button>(serviceProvider)
    {
        private readonly Lazy<Dictionary<string, ButtonInstanceDescriptor?>> buttonDescriptors = new(() => []);

        internal IEnumerable<IGrouping<string, ButtonInstanceDescriptor>> GetButtons()
        {
            var dict = buttonDescriptors.Value;
            while (_operations.TryDequeue(out var op))
            {
                switch (op)
                {
                    case Operation.Assign assign:
                        var button = assign.Parse();
                        if (button != null)
                        {
                            button.DataContext = new DockingWindowRibbonButtonViewModel(assign.Desc.AnchorableViewModelType);
                        }
                        dict[assign.Key] = new ButtonInstanceDescriptor(assign.Desc.GroupKey, button);
                        break;
                    case Operation.Remove remove:
                        dict.Remove(remove.Key);
                        break;
                    default:
                        break;
                }
            }
            return dict.Values
                .OfType<ButtonInstanceDescriptor>()
                .GroupBy(bd => bd.Group);
        }

        public IEnumerable<Type> GetAllAvailableTypes()
        {
            return GetRegisteredAvailableData().Select(d => d.Value.data.AnchorableViewModelType);
        }
    }
}
