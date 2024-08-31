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

                var files = context.ServiceProvider.GetRequiredService<ResourceGatheringServiceProvider>()
                    .GetResourcesToPack(root, context.LocalParam)
                    .Where(groupFilter);
                resourcePath.AddRange(files.Select(gr => gr.Path));
                resourceNameInPackage.AddRange(files.Select(gr => gr.TargetName));
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();
            }
            ResourcePathVariable.WriteTarget(resourcePath, context);
            ResourceNameInPackageVariable.WriteTarget(resourceNameInPackage, context);
        }
    }
}
