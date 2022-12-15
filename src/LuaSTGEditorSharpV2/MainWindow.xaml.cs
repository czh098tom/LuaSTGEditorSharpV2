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
using System.Windows.Navigation;
using System.IO;

using Newtonsoft.Json;

using Xceed.Wpf.AvalonDock.Controls;

using LuaSTGEditorSharpV2.Core.ViewModel;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string testPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\test");

            try
            {
                string testSrc;
                using (FileStream fs = new(Path.Combine(testPath, "test.lstg"), FileMode.Open, FileAccess.Read))
                {
                    using StreamReader sr = new(fs);
                    testSrc = sr.ReadToEnd();
                }
                NodeData root = JsonConvert.DeserializeObject<NodeData>(testSrc) ?? throw new Exception();
                DocumentViewModel dvm = new();

                DataContext = dvm;

                ViewModelProviderServiceBase.CreateRoot(dvm.Tree, root);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
