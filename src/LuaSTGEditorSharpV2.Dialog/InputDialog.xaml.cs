﻿using System;
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

using LuaSTGEditorSharpV2.Dialog.ViewModel;
using LuaSTGEditorSharpV2.WPF;

namespace LuaSTGEditorSharpV2
{
    /// <summary>
    /// InputDialog.xaml 的交互逻辑
    /// </summary>
    public partial class InputDialog : OKCancelWindow
    {
        public InputDialogViewModel ViewModel
            => (DataContext as InputDialogViewModel) ?? throw new InvalidCastException();

        public InputDialog()
        {
            InitializeComponent();
        }
    }
}
