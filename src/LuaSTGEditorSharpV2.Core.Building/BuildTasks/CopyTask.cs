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

        public async Task Execute(BuildingContext context, IProgress<float>? progressReporter = null, CancellationToken cancellationToken = default)
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
                    await CopyWithRetry(source, targetName, outputPath, cancellationToken);
                }
            }
        }

        private static async Task CopyWithRetry(string source, string targetName, string outputPath, CancellationToken cancellationToken)
        {
            int retryCount = 5;
            while (retryCount > 0)
            {
                try
                {
                    if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
                    var target = Path.Combine(outputPath, targetName);
                    File.Copy(source, target, true);
                    retryCount = 0;
                }
                catch (IOException e)
                {
                    Console.WriteLine(e);
                    retryCount--;
                    if (retryCount < 0) throw;
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }
    }
}
