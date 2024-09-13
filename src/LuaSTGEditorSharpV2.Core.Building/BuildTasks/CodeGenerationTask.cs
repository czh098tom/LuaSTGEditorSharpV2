using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core.CodeGenerator;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTasks
{
    public class CodeGenerationTask : IBuildingTask
    {
        public IInputSourceVariable Source { get; private set; }
        public IOutputTargetVariable? TargetVariable { get; private set; }

        public CodeGenerationTask(IInputSourceVariable source, IOutputTargetVariable? targetVariable = null)
        {
            Source = source;
            TargetVariable = targetVariable;
        }

        public async Task Execute(BuildingContext context,
            IProgress<ProgressReportingParam>? progressReporter = null, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<string> path = Source.GetVariable(context);
            List<string> targets = [];
            foreach (string pathItem in path)
            {
                var targetName = await GenerateForPath(context, pathItem, cancellationToken);
                if (targetName != null)
                {
                    targets.Add(targetName);
                }
            }
            TargetVariable?.WriteTarget(targets, context);
        }

        private static async Task<string?> GenerateForPath(BuildingContext context, string pathItem, CancellationToken cancellationToken)
        {
            var doc = DocumentModel.CreateFromFile(pathItem);
            if (doc == null) return null;
            NodeData? compileRoot = doc.FindCompileRoot();
            if (compileRoot == null) return null;

            var targetName = context.TempFiles.AcquireTempFile();
            using (FileStream fs = new(targetName, FileMode.Create, FileAccess.Write))
            {
                using StreamWriter sw = new(fs, Encoding.UTF8);
                foreach (CodeData codeData in context.ServiceProvider.GetRequiredService<CodeGeneratorServiceProvider>()
                    .GenerateCode(compileRoot, context.LocalParam))
                {
                    sw.Write(codeData.Content);
                    await Task.Yield();
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            return targetName;
        }
    }
}
