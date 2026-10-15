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
        public bool KeepFirstTarget { get; set; }

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
            if (sourcePaths.Count != targetNames.Count)
            {
                throw new InvalidOperationException("Copy source and target counts must match.");
            }
            var copiedTargets = new Dictionary<string, string>(OperatingSystem.IsWindows()
                ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

            foreach (var (source, targetName) in sourcePaths.Zip(targetNames))
            {
                foreach (var outputPath in outputBasePaths)
                {
                    await Task.Yield();
                    cancellationToken.ThrowIfCancellationRequested();
                    var target = GetTargetPath(outputPath, targetName);
                    if (copiedTargets.TryGetValue(target, out var previousSource))
                    {
                        context.LogWriter.WriteLine($"[CopyTask] Duplicate target \"{target}\": \"{previousSource}\" and \"{source}\". "
                            + (KeepFirstTarget ? "Keeping the first source." : "Overwriting the previous source."));
                        if (KeepFirstTarget) continue;
                    }
                    await CopyWithRetry(source, target, context, cancellationToken);
                    copiedTargets[target] = source;
                }
            }
        }

        private static string GetTargetPath(string outputPath, string targetName)
        {
            if (string.IsNullOrWhiteSpace(targetName) || Path.IsPathRooted(targetName)
                || targetName.StartsWith('\\') || targetName.Contains(':'))
            {
                throw new InvalidOperationException($"Copy target '{targetName}' must be a relative file path.");
            }
            var normalizedName = targetName.Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);
            var parts = normalizedName.Split(Path.DirectorySeparatorChar);
            if (parts[^1] is "" or "." or ".."
                || parts.Any(part => part.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
                    || (part is not "." and not ".." && (part.EndsWith(' ') || part.EndsWith('.')))))
            {
                throw new InvalidOperationException($"Copy target '{targetName}' is not a valid file path.");
            }
            var outputRoot = Path.GetFullPath(outputPath);
            var target = Path.GetFullPath(normalizedName, outputRoot);
            var rootPrefix = Path.EndsInDirectorySeparator(outputRoot)
                ? outputRoot : outputRoot + Path.DirectorySeparatorChar;
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (!target.StartsWith(rootPrefix, comparison))
            {
                throw new InvalidOperationException($"Copy target '{targetName}' escapes output directory '{outputRoot}'.");
            }
            return target;
        }

        private static async Task CopyWithRetry(string source, string target, BuildingContext context, CancellationToken cancellationToken)
        {
            for (var attempt = 1; ; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                    File.Copy(source, target, true);
                    context.LogWriter.WriteLine($"[CopyTask] Copied \"{source}\" to \"{target}\"");
                    return;
                }
                catch (IOException e)
                {
                    if (attempt == 5)
                    {
                        context.LogWriter.WriteLine($"[CopyTask] Copying \"{source}\" to \"{target}\" failed after {attempt} attempts. Exception: \n{e}");
                        throw;
                    }
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }
    }
}
