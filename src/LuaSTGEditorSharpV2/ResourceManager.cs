using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using LuaSTGEditorSharpV2.PropertyView;

namespace LuaSTGEditorSharpV2
{
    public class ResourceManager
    {
        public static void MergeResources()
        {
            foreach (string uri in PropertyViewServiceBase.ResourceDictUris)
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
