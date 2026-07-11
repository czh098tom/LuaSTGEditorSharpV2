using global::LinqSTG.Kinematics;
using System.Collections.Generic;
using System.Numerics;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows
{
    public class PointShooter<TData>(
        Func<TData?, IParametric<int, Vector2>?> createPrediction,
        BulletShape shape,
        float diameter)
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
                        yield return new PointPrediction(pred, startTime, shape, diameter);
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
