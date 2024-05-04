using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.Building
{
    public class CompositeBuildingTask : IBuildingTask
    {
        public static IBuildingTask? LeastObjectCombine(params IBuildingTask?[] tasks)
        {
            var skipNull = new List<IBuildingTask>(tasks.Length);
            foreach (var task in tasks)
            {
                if (task != null)
                {
                    skipNull.Add(task);
                }
            }
            if (skipNull.Count > 1)
            {
                return new CompositeBuildingTask(skipNull.ToArray());
            }
            else if (skipNull.Count > 0)
            {
                return skipNull[0];
            }
            else
            {
                return null;
            }
        }

        public static WeightedBuildingTask? LeastObjectCombine(params WeightedBuildingTask?[] tasks)
        {
            var skipNull = new List<WeightedBuildingTask>(tasks.Length);
            foreach (var task in tasks)
            {
                if (task != null)
                {
                    skipNull.Add(task.Value);
                }
            }
            if (skipNull.Count > 1)
            {
                return new WeightedBuildingTask(new CompositeBuildingTask(skipNull.ToArray()), 1);
            }
            else if (skipNull.Count > 0)
            {
                return skipNull[0];
            }
            else
            {
                return null;
            }
        }

        private class WeightedProgressWrapper(float offset, float step)
            : IProgress<ProgressReportingParam>
        {
            public IProgress<ProgressReportingParam>? inner;

            public void Report(ProgressReportingParam value)
            {
                inner?.Report(new ProgressReportingParam(
                    offset + value.Percentage * step, value.NameKey));
            }
        }

        public IBuildingTask[] BuildingTasks { get; private set; }
        private readonly IReadOnlyDictionary<IBuildingTask, WeightedProgressWrapper> _wrappers;

        private bool executing = false;

        public CompositeBuildingTask(params IBuildingTask[] buildingTasks)
        {
            BuildingTasks = buildingTasks;
            var wrappers = new Dictionary<IBuildingTask, WeightedProgressWrapper>();
            var step = 1f / buildingTasks.Length;
            for (int i = 0; i < buildingTasks.Length; i++)
            {
                var buildingTask = buildingTasks[i];
                wrappers.Add(buildingTask, new WeightedProgressWrapper(i * step, step));
            }
            _wrappers = wrappers;
        }

        public CompositeBuildingTask(params WeightedBuildingTask[] buildingTasks)
        {
            BuildingTasks = buildingTasks.Select(t => t.BuildingTask).ToArray();
            var wrappers = new Dictionary<IBuildingTask, WeightedProgressWrapper>();
            var sum = 0f;
            for (int i = 0; i < buildingTasks.Length; i++)
            {
                sum += buildingTasks[i].Weight;
            }
            var curr = 0f;
            for (int i = 0; i < buildingTasks.Length; i++)
            {
                var buildingTask = buildingTasks[i].BuildingTask;
                var step = buildingTasks[i].Weight / sum;
                wrappers.Add(buildingTask, new WeightedProgressWrapper(curr, step));
                curr += step;
            }
            _wrappers = wrappers;
        }

        public async Task Execute(BuildingContext context,
            IProgress<ProgressReportingParam>? progressReporter = null, CancellationToken cancellationToken = default)
        {
            if (executing) throw new InvalidOperationException("A task has already been executing");
            try
            {
                executing = true;
                foreach (var task in BuildingTasks)
                {
                    var reporter = _wrappers[task];
                    reporter.inner = progressReporter;
                    await task.Execute(context, reporter, cancellationToken);
                }
            }
            finally
            {
                executing = false;
            }
        }
    }
}
