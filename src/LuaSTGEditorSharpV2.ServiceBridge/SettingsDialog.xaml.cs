using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.ServiceBridge.Services;
using LuaSTGEditorSharpV2.ServiceBridge.ViewModel;
using LuaSTGEditorSharpV2.WPF;

namespace LuaSTGEditorSharpV2.ServiceBridge
{
    /// <summary>
    /// SettingsDialog.xaml 的交互逻辑
    /// </summary>
    [Inject(ServiceLifetime.Transient)]
    public partial class SettingsDialog : OKCancelWindow
    {
        public SettingsDialog(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            var ctx = serviceProvider.GetRequiredService<SettingsDialogViewModel>();

            // TODO: optimize this
            var selector = serviceProvider.GetRequiredService<SettingsPageTemplateSelector>();
            selector.Default = Resources["SettingsPageTemplateSelectorDefault"] as DataTemplate;
            Resources["SettingsPageTemplateSelector"] = selector;

            DataContext = ctx;

            Confirmed += (o, e) =>
            {
                if (ctx == null) return;
                serviceProvider.GetRequiredService<SettingsDisplayService>().WriteViewModelBack(ctx.SettingsPages);
                serviceProvider.GetRequiredService<SettingsService>().SaveSettings();
            };
        }
    }
}
