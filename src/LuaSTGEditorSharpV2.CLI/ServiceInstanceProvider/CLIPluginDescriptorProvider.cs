using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.CLI.Plugin;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Building.BuildTaskFactory;
using LuaSTGEditorSharpV2.Core.Building.BuildTasks;
using LuaSTGEditorSharpV2.Core.Building;
using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.CLI.ServiceInstanceProvider
{
    [Inject(ServiceLifetime.Transient)]
    public class CLIPluginDescriptorProvider : IServiceInstanceProvider<CLIPluginDescriptor>
    {
        public IReadOnlyCollection<CLIPluginDescriptor> GetServiceInstances(IServiceProvider serviceProvider)
        {
            return
            [
                new CLIPluginDescriptor("compile", async (param) =>
                {
                    if (param.InputPath != null)
                    {
                        var doc = DocumentModel.CreateFromFile(param.InputPath);
                        if (doc == null) return;
                        NodeData? compileRoot = doc.FindCompileRoot();
                        if (compileRoot == null) return;

                        TextWriter writer;
                        if (param.OutputPath == null)
                        {
                            writer = Console.Out;

                            foreach (CodeData codeData in serviceProvider.GetRequiredService<CodeGeneratorServiceProvider>()
                                .GenerateCode(compileRoot, new LocalServiceParam(doc)))
                            {
                                await writer.WriteAsync(codeData.Content);
                            }
                        }
                        else
                        {
                            using FileStream fs = new(param.OutputPath, FileMode.Create, FileAccess.Write);
                            using StreamWriter sr = new(fs);
                            writer = sr;

                            foreach (CodeData codeData in serviceProvider.GetRequiredService<CodeGeneratorServiceProvider>()
                                .GenerateCode(compileRoot, new LocalServiceParam(doc)))
                            {
                                await writer.WriteAsync(codeData.Content);
                            }
                        }

                        //DocumentFormatBase.Create("xml").SaveToStream(root, Console.Out);
                        //Console.WriteLine();
                    }

                }, serviceProvider),
                new CLIPluginDescriptor("formatxml", (param) =>
                {
                    if (param.InputPath != null)
                    {
                        var doc = DocumentModel.CreateFromFile(param.InputPath);
                        if (doc == null) return Task.CompletedTask;
                        NodeData? root = doc.Root;

                        TextWriter writer;
                        if (param.OutputPath == null)
                        {
                            writer = Console.Out;
                            DocumentFormatBase.Create("xml").SaveToStream(root, writer);
                        }
                        else
                        {
                            using FileStream fs = new(param.OutputPath, FileMode.OpenOrCreate);
                            using StreamWriter sr = new(fs);
                            writer = sr;
                            DocumentFormatBase.Create("xml").SaveToStream(root, writer);
                        }
                    }
                    return Task.CompletedTask;
                }, serviceProvider),
                new CLIPluginDescriptor("build", async (param) =>
                {
                    if (param.InputPath != null)
                    {
                        var doc = DocumentModel.CreateFromFile(param.InputPath);
                        if (doc == null) return;
                        NodeData? buildRoot = doc.FindBuildRoot();
                        if (buildRoot == null) return;
                        var service = serviceProvider.GetRequiredService<BuildTaskFactoryServiceProvider>();
                        var tasks = buildRoot.GetLogicalChildren()
                            .Select(n => service.GetWeightedBuildingTaskForNode(n, new LocalServiceParam(doc))?.BuildingTask)
                            .OfType<NamedBuildingTask>()
                            .Where(nbt => nbt.Name == param.TaskName);
                        foreach (var task in tasks)
                        {
                            using var ctx = serviceProvider.GetRequiredService<BuildingContextFactory>()
                                .Create(new LocalServiceParam(doc));
                            try
                            {
                                await task.Execute(ctx);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                    }
                }, serviceProvider),
            ];
        }
    }
}
