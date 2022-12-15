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
using LuaSTGEditorSharpV2.PackageManagement;

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

            string testPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\test");
            try
            {
                PackageManager.UseService(typeof(CodeGeneratorServiceBase));
                PackageManager.UseService(typeof(ViewModelProviderServiceBase));
                PackageManager.LoadPackage(Path.Combine(testPath, "package"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
