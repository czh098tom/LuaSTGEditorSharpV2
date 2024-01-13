using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LuaSTGEditorSharpV2.WPF
{
    public class OKCancelWindow : Window
    {
        public event EventHandler? Confirmed;
        public event EventHandler? Canceled;

        protected void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
            Confirmed?.Invoke(this, EventArgs.Empty);
        }

        protected void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Canceled?.Invoke(this, EventArgs.Empty);
        }
    }
}
