using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

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
                HostedApplicationHelper.InitNodeService();
                var nodePackageProvider = HostedApplicationHelper.GetService<NodePackageProvider>();
                var resc = nodePackageProvider.LoadPackage("Core");
                var lua = nodePackageProvider.LoadPackage("Lua");
                var resln = nodePackageProvider.LoadPackage("LegacyNode");
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
