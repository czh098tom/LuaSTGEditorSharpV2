using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2
{
    public class MainWorker : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
            return Task.CompletedTask;
        }
    }
}
