using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core.Building.ResourceGathering;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTasks
{
    public class CaptureResourceGroupTask : IBuildingTask
    {
        private static readonly Func<GroupedResource, bool> _unitFilter = gr => true;

        public IInputSourceVariable SourceDocumentPathVariable { get; private set; }
        public IOutputTargetVariable ResourcePathVariable { get; private set; }
        public IOutputTargetVariable ResourceNameInPackageVariable { get; private set; }

        public string? InitialResourceGroup { get; set; }

        private readonly Func<GroupedResource, bool> groupFilter;

        public CaptureResourceGroupTask(IInputSourceVariable source, IOutputTargetVariable path,
            IOutputTargetVariable nameInPackage, Predicate<string>? groupFilter = null)
        {
            SourceDocumentPathVariable = source;
            ResourcePathVariable = path;
            ResourceNameInPackageVariable = nameInPackage;
            if (groupFilter == null)
            {
                this.groupFilter = _unitFilter;
            }
            else
            {
                this.groupFilter = gr => groupFilter(gr.ResourceGroup);
            }
        }

        public async Task Execute(BuildingContext context, IProgress<ProgressReportingParam>? progressReporter = null,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<string> documentPaths = SourceDocumentPathVariable.GetVariable(context);
            List<string> resourcePath = [];
            List<string> resourceNameInPackage = [];
            foreach (string documentPath in documentPaths)
            {
                var doc = DocumentModel.CreateFromFile(documentPath);
                if (doc == null) continue;
                NodeData root = doc.Root;

                var provider = context.ServiceProvider.GetRequiredService<ResourceGatheringServiceProvider>();
                var documentDirectory = Path.GetDirectoryName(Path.GetFullPath(documentPath))!;
                var files = provider.GetResourcesToPack(root, new LocalServiceParam(doc),
                    (ResourceGatheringServiceSettings)provider.Settings, InitialResourceGroup)
                    .Where(groupFilter);
                foreach (var file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    resourcePath.Add(Path.GetFullPath(file.Path, documentDirectory));
                    resourceNameInPackage.Add(file.TargetName);
                }
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();
            }
            ResourcePathVariable.WriteTarget(resourcePath, context);
            ResourceNameInPackageVariable.WriteTarget(resourceNameInPackage, context);
        }
    }
}
