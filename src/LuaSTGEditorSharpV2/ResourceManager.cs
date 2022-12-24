using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LuaSTGEditorSharpV2
{
    public class ResourceManager
    {
        private static readonly string[] builtInUris = new string[]
        {
            "pack://application:,,,/LuaSTGEditorSharpV2.PropertyView;component/BasicPropertyView.xaml"
        };

        public static void MergeResources()
        {
            foreach (string uri in builtInUris)
            {
                ResourceDictionary dictionary = new()
                {
                    Source = new Uri(uri)
                };
                Application.Current.Resources.MergedDictionaries.Add(dictionary);
            }
        }
    }
}
