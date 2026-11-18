using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.Noesis;

namespace ViewModelsSamples.Events.OverrideFind;

public class ViewModel
{
    public ISeries[] Series { get; set; } = [
        new CustomColumnSeries<int> { Values = [9, 5, 7, 3, 7, 3] },
        new CustomColumnSeries<int> { Values = [8, 2, 3, 2, 5, 2] }
    ];

    public class CustomColumnSeries<T> : ColumnSeries<T>
    {
        protected override IEnumerable<ChartPoint> FindPointsInPosition(
            Chart chart, LvcPoint pointerPosition, FindingStrategy strategy, FindPointFor findPointFor)
        {
            return Fetch(chart).Where(point =>
            {
                var ha = (RectangleHoverArea?)point.Context.HoverArea;
                if (ha is null) return false;

                var isInsideX = ha.X <= pointerPosition.X && pointerPosition.X <= ha.X + ha.Width;
                var isInsideY = ha.Y <= pointerPosition.Y && pointerPosition.Y <= ha.Y + ha.Height;

                return findPointFor == FindPointFor.HoverEvent
                    ? isInsideX
                    : isInsideY;
            });
        }
    }
}
