using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTasks
{
    public class CopyTask : IBuildingTask
    {
        public IInputSourceVariable SourceVariable { get; set; }
        public IInputSourceVariable TargetNameVariable { get; set; }
        public IInputSourceVariable ArchivePathVariable { get; set; }

        public CopyTask(IInputSourceVariable sourceVariable, IInputSourceVariable targetNameVariable,
            IInputSourceVariable archivePathVariable)
        {
            SourceVariable = sourceVariable;
            TargetNameVariable = targetNameVariable;
            ArchivePathVariable = archivePathVariable;
        }

        public async Task Execute(BuildingContext context, IProgress<ProgressReportingParam>? progressReporter = null, CancellationToken cancellationToken = default)
        {
            var outputBasePaths = ArchivePathVariable.GetVariable(context);
            var sourcePaths = SourceVariable.GetVariable(context);
            var targetNames = TargetNameVariable.GetVariable(context);

            foreach (var (source, targetName) in sourcePaths.Zip(targetNames))
            {
                foreach (var outputPath in outputBasePaths)
                {
                    await Task.Yield();
                    cancellationToken.ThrowIfCancellationRequested();
                    await CopyWithRetry(source, targetName, outputPath, context, cancellationToken);
                }
            }
        }

        private static async Task CopyWithRetry(string source, string targetName, string outputPath, BuildingContext context, CancellationToken cancellationToken)
        {
            int retryCount = 5;
            var target = Path.Combine(outputPath, targetName);
            while (retryCount > 0)
            {
                try
                {
                    if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
                    File.Copy(source, target, true);
                    context.LogWriter.WriteLine($"[CopyTask] Copied \"{source}\" to \"{target}\"");
                    retryCount = 0;
                }
                catch (IOException e)
                {
                    Console.WriteLine(e);
                    retryCount--;
                    if (retryCount < 0)
                    {
                        context.LogWriter.WriteLine($"[CopyTask] Copying \"{source}\" to \"{target}\" failed. Exception: \n{e}");
                        throw;
                    }
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }
    }
}
