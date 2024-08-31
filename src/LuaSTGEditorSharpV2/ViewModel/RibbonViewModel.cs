using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class RibbonViewModel : InjectableViewModel
    {
        public InsertPanelViewModel InsertPanel { get; }

        public QueuedBoolHandle IsEnabledHandle { get; private set; }

        public bool IsEnabled
        {
            get => IsEnabledHandle.Value;
        }

        public RibbonViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            InsertPanel = serviceProvider.GetRequiredService<InsertPanelViewModel>();
            IsEnabledHandle = new(this, nameof(IsEnabled));
        }
    }
}
