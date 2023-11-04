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
using LuaSTGEditorSharpV2.PropertyView;
using System.Reflection;
using LuaSTGEditorSharpV2.ViewModel;

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
                ServiceManager.UseService(typeof(CodeGeneratorServiceBase));
                ServiceManager.UseService(typeof(ViewModelProviderServiceBase));
                ServiceManager.UseService(typeof(PropertyViewServiceBase));
                var resc = ServiceManager.LoadPackage("Core");
                var lua = ServiceManager.LoadPackage("Lua");
                var resln = ServiceManager.LoadPackage("LegacyNode");
                ResourceManager.MergeResources();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
