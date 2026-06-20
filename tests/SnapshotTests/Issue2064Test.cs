using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace SnapshotTests;

// Issue #2064: when only Stroke is set on a LineSeries, the legend miniature
// and per-point markers used to take theme-rotated palette colors keyed by
// SeriesId — so a Blue/Green/Red line ended up with a Blue/Red/LightGreen
// marker dot, giving each series two identifying colors.
//
// The snapshot below reproduces the reporter's exact 3-series scenario and
// asserts the rendered marker rings now match the user-set Stroke colors.
[TestClass]
public class Issue2064Test
{
    [TestMethod]
    public void MarkerColorMatchesUserStroke()
    {
        var chart = new SKCartesianChart
        {
            Width = 600,
            Height = 400,
            Series =
            [
                new LineSeries<int>
                {
                    Name = "qty",
                    Values = [10, 12, 11, 14, 13],
                    Stroke = new SolidColorPaint(SKColors.Blue, 4)
                },
                new LineSeries<int>
                {
                    Name = "ok",
                    Values = [8, 9, 7, 10, 9],
                    Stroke = new SolidColorPaint(SKColors.Green, 4)
                },
                new LineSeries<int>
                {
                    Name = "ng",
                    Values = [2, 3, 4, 1, 3],
                    Stroke = new SolidColorPaint(SKColors.Red, 4)
                }
            ],
            LegendPosition = LiveChartsCore.Measure.LegendPosition.Top
        };

        chart.AssertSnapshotMatches($"{nameof(Issue2064Test)}_{nameof(MarkerColorMatchesUserStroke)}");
    }
}
