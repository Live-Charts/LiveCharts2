using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;

namespace SnapshotTests;

[TestClass]
public sealed class HeatSeriesTests
{
    [TestMethod]
    public void Basic()
    {
        var xLabels = new[] { "Charles", "Richard", "Ana", "Mari" };
        var yLabels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
        var values = new WeightedPoint[]
        {
            // Charles
            new(0, 0, 150),
            new(0, 1, 123),
            new(0, 2, 310),
            new(0, 3, 225),
            new(0, 4, 473),
            new(0, 5, 373),
            // Richard
            new(1, 0, 432),
            new(1, 1, 312),
            new(1, 2, 135),
            new(1, 3, 78),
            new(1, 4, 124),
            new(1, 5, 423),
            // Ana
            new(2, 0, 543),
            new(2, 1, 134),
            new(2, 2, 524),
            new(2, 3, 315),
            new(2, 4, 145),
            new(2, 5, 80),
            // Mari
            new(3, 0, 90),
            new(3, 1, 123),
            new(3, 2, 70),
            new(3, 3, 123),
            new(3, 4, 432),
            new(3, 5, 142)
        };

        var series = new ISeries[]
        {
            new HeatSeries<WeightedPoint>
            {
                Values = values,
                HeatMap = [
                    SKColor.Parse("#FFF176").AsLvcColor(),
                    SKColor.Parse("#2F4F4F").AsLvcColor(),
                    SKColor.Parse("#0000FF").AsLvcColor()
                ]
            }
        };

        var xAxis = new Axis { Labels = xLabels };
        var yAxis = new Axis { Labels = yLabels };

        var chart = new SKCartesianChart
        {
            Series = series,
            XAxes = [xAxis],
            YAxes = [yAxis],
            LegendPosition = LiveChartsCore.Measure.LegendPosition.Right,
            Legend = new SKHeatLegend { BadgePadding = new LiveChartsCore.Drawing.Padding(30, 20, 8, 20) },
            Width = 600,
            Height = 600
        };

        chart.AssertSnapshotMatches($"{nameof(HeatSeriesTests)}_{nameof(Basic)}");
    }

    // Regression for https://github.com/Live-Charts/LiveCharts2/issues/1511
    // Continuous Y axis with 0.1 data step on a 0.5..1.0 range. Before the fix,
    // cells were sized to Axis.UnitWidth (default 1) and overlapped each other,
    // so the chart collapsed into an X-only gradient with no visible Y variation.
    [TestMethod]
    public void Issue1511_ContinuousYAxis_FractionalStep()
    {
        var values = new List<WeightedPoint>();
        for (var x = 0; x < 16; x++)
        {
            for (var i = 0; i < 6; i++)
            {
                var y = 0.5 + i * 0.1;
                var t = (x + (5 - i)) / 20.0;
                var w = 1 + (int)((1 - t) * 399);
                values.Add(new WeightedPoint(x, y, w));
            }
        }

        var chart = new SKCartesianChart
        {
            Series = [
                new HeatSeries<WeightedPoint>
                {
                    Values = values,
                    HeatMap = [
                        new SKColor(0xfff27a7d).AsLvcColor(),
                        new SKColor(0xfff7d486).AsLvcColor(),
                        new SKColor(0xffc5f9d7).AsLvcColor(),
                    ],
                    ColorStops = [0, 0.5, 1],
                    PointPadding = new LiveChartsCore.Drawing.Padding(0)
                }
            ],
            XAxes = [new Axis { MinLimit = 0, MaxLimit = 15 }],
            YAxes = [new Axis { MinLimit = 0.5, MaxLimit = 1.0 }],
            Width = 600,
            Height = 400
        };

        chart.AssertSnapshotMatches(
            $"{nameof(HeatSeriesTests)}_{nameof(Issue1511_ContinuousYAxis_FractionalStep)}");
    }

    // Regression for https://github.com/Live-Charts/LiveCharts2/issues/1511
    // Same fractional-step data as above but with no explicit Min/MaxLimit on either
    // axis. Before the GetBounds fix, base.GetBounds padded by Axis.UnitWidth=1 each
    // side, so a Y axis covering 0.5..1.0 expanded to roughly 0..1.5 with most of the
    // chart left empty. After the fix it pads by the computed cell step (0.1 on Y,
    // 1 on X), keeping the cells flush to the draw margin.
    [TestMethod]
    public void Issue1511_AutoAxisBounds_FractionalStep()
    {
        var values = new List<WeightedPoint>();
        for (var x = 0; x < 16; x++)
        {
            for (var i = 0; i < 6; i++)
            {
                var y = 0.5 + i * 0.1;
                var t = (x + (5 - i)) / 20.0;
                var w = 1 + (int)((1 - t) * 399);
                values.Add(new WeightedPoint(x, y, w));
            }
        }

        var chart = new SKCartesianChart
        {
            Series = [
                new HeatSeries<WeightedPoint>
                {
                    Values = values,
                    HeatMap = [
                        new SKColor(0xfff27a7d).AsLvcColor(),
                        new SKColor(0xfff7d486).AsLvcColor(),
                        new SKColor(0xffc5f9d7).AsLvcColor(),
                    ],
                    ColorStops = [0, 0.5, 1],
                    PointPadding = new LiveChartsCore.Drawing.Padding(0)
                }
            ],
            Width = 600,
            Height = 400
        };

        chart.AssertSnapshotMatches(
            $"{nameof(HeatSeriesTests)}_{nameof(Issue1511_AutoAxisBounds_FractionalStep)}");
    }

    // Regression for https://github.com/Live-Charts/LiveCharts2/pull/2196 review.
    // Empty points carry Coordinate(0, 0). If they aren't skipped while deriving
    // cell steps, the spurious 0 in the distinct-values set can land closer to
    // real data than the natural step and shrink the computed step. Here Y data
    // is half-integer (0.5..5.5 by 1), so the empty's 0 sits 0.5 from the lowest
    // Y — half the real step — and unfixed code would size cells at half height.
    [TestMethod]
    public void Issue1511_EmptyPointsDoNotContaminateStep()
    {
        var values = new List<WeightedPoint>
        {
            new(null, null, null) // empty point contributes Coordinate(0, 0)
        };
        for (var x = 0; x < 6; x++)
        {
            for (var i = 0; i < 6; i++)
            {
                var y = 0.5 + i;
                var w = 1 + (x + i) * 10;
                values.Add(new WeightedPoint(x, y, w));
            }
        }

        var chart = new SKCartesianChart
        {
            Series = [
                new HeatSeries<WeightedPoint>
                {
                    Values = values,
                    HeatMap = [
                        new SKColor(0xfff27a7d).AsLvcColor(),
                        new SKColor(0xffc5f9d7).AsLvcColor(),
                    ],
                    PointPadding = new LiveChartsCore.Drawing.Padding(0)
                }
            ],
            XAxes = [new Axis { MinLimit = -0.5, MaxLimit = 5.5 }],
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 6 }],
            Width = 600,
            Height = 400
        };

        chart.AssertSnapshotMatches(
            $"{nameof(HeatSeriesTests)}_{nameof(Issue1511_EmptyPointsDoNotContaminateStep)}");
    }
}
