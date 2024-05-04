using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building.BuildTasks
{
    public static class BuildTaskHelper
    {
        private class WrapContextTask(IBuildingTask inner) : IBuildingTask
        {
            private readonly IBuildingTask _inner = inner;

            public async Task Execute(BuildingContext context,
                IProgress<ProgressReportingParam>? progressReporter = null,
                CancellationToken cancellationToken = default)
            {
                using var innerContext = new BuildingContext(context);
                await _inner.Execute(innerContext, progressReporter, cancellationToken);
            }
        }

        public static IBuildingTask WrapContext(this IBuildingTask inner)
        {
            return new WrapContextTask(inner);
        }

        public static IBuildingTask WithNameIfValid(this IBuildingTask inner, string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return inner;
            }
            else
            {
                return new NamedBuildingTask(name, inner);
            }
        }

        public static WeightedBuildingTask WithNameIfValid(this WeightedBuildingTask inner, string? name)
        {
            return new WeightedBuildingTask(inner.BuildingTask.WithNameIfValid(name), inner.Weight);
        }
    }
}
