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
    public partial class SettingsDialog : OKCancelWindow
    {
        public SettingsDialog()
        {
            InitializeComponent();
            Confirmed += (o, e) =>
            {
                var ctx = DataContext as SettingsDialogViewModel;
                if (ctx == null) return;
                HostedApplicationHelper.GetService<SettingsDisplayService>()
                    .WriteViewModelBack(ctx.SettingsPages);
                HostedApplicationHelper.GetService<SettingsService>().SaveSettings();
            };
        }
    }
}
