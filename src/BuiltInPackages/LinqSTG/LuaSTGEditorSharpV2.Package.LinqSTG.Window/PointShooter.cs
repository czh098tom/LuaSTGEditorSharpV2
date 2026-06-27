using global::LinqSTG.Kinematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows
{
    public class PointShooter<TData>(Func<TData?, IParametric<int, Vector2>?> createPrediction)
        : IShooter<TData, int, IEnumerable<PointPrediction>>
    {
        public IEnumerable<PointPrediction> Shoot(IPattern<TData, int>? pattern)
        {
            var startTime = 0;
            if (pattern is null || !pattern.Any() || createPrediction is null)
            {
                yield break;
            }
            foreach (var data in pattern)
            {
                if (data.IsData)
                {
                    var pred = createPrediction(data.Data);
                    if (pred != null)
                    {
                        yield return new PointPrediction(pred, startTime);
                    }
                }
                else
                {
                    startTime += data.Interval;
                }
            }
        }
    }
}
