using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.ViewModel;
using LuaSTGEditorSharpV2.PropertyView;
using System.Reflection;

namespace LuaSTGEditorSharpV2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var dt = Resources["PropertyDataTemplates"];

            string testPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\test");
            try
            {
                PackageManager.UseService(typeof(CodeGeneratorServiceBase));
                PackageManager.UseService(typeof(ViewModelProviderServiceBase));
                PackageManager.UseService(typeof(PropertyViewServiceBase));
                var resc = PackageManager.LoadPackage("Core");
                var resln = PackageManager.LoadPackage("LegacyNode");
                ResourceManager.MergeResources();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
