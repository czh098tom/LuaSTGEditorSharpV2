using LuaSTGEditorSharpV2.Dialog.ViewModel;
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

namespace LuaSTGEditorSharpV2.Dialog
{
    /// <summary>
    /// ViewCodeDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ViewCodeDialog : Window
    {
        public ViewCodeDialog()
        {
            InitializeComponent();
        }

        public ViewCodeDialog(string text)
        {
            InitializeComponent();
            textEditor.Text = text;
        }
    }
}
