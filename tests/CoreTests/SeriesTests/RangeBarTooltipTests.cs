using System;
using System.Globalization;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.SeriesTests;

// Pins the contract of CoreRangeColumnSeries.GetPrimaryToolTipText and
// CoreRangeRowSeries.GetPrimaryToolTipText:
//
//   1. User-provided XToolTipLabelFormatter / YToolTipLabelFormatter wins.
//   2. Otherwise the default formats both Low + High through the VALUE
//      axis labeler (Y for column, X for row) — so a DateTimeAxis renders
//      dates instead of raw Ticks.
[TestClass]
public class RangeBarTooltipTests
{
    private static void Measure(SKCartesianChart chart)
    {
        var core = (CartesianChartEngine)chart.CoreChart;
        CoreMotionCanvas.DebugElapsedMilliseconds = 0;
        core.IsLoaded = true;
        core._isFirstDraw = true;
        core.Measure();
    }


    [TestMethod]
    public void RangeColumn_DefaultTooltip_UsesValueAxisLabeler()
    {
        var series = new RangeColumnSeries<RangeValue>
        {
            Values = [new(10, 30)],
        };
        var chart = new SKCartesianChart
        {
            Width = 200,
            Height = 200,
            Series = [series],
            YAxes = [new Axis { Labeler = v => $"${v}", MinLimit = 0, MaxLimit = 50 }],
        };

        Measure(chart);

        var point = series.everFetched.Single();
        var tooltip = series.GetPrimaryToolTipText(point);

        Assert.AreEqual("$10 → $30", tooltip);
    }

    [TestMethod]
    public void RangeColumn_DateTimeAxis_RendersDatesInTooltip()
    {
        var start = new DateTime(2026, 6, 1);
        var end = new DateTime(2026, 6, 5);
        var series = new RangeColumnSeries<RangeValue>
        {
            Values = [new(start.Ticks, end.Ticks)],
        };
        var chart = new SKCartesianChart
        {
            Width = 200,
            Height = 200,
            Series = [series],
            YAxes = [new DateTimeAxis(TimeSpan.FromDays(1), d => d.ToString("MMM dd", CultureInfo.InvariantCulture))],
        };

        Measure(chart);

        var point = series.everFetched.Single();
        var tooltip = series.GetPrimaryToolTipText(point);

        Assert.AreEqual("Jun 01 → Jun 05", tooltip);
    }

    [TestMethod]
    public void RangeColumn_UserFormatter_TakesPrecedence()
    {
        var series = new RangeColumnSeries<RangeValue>
        {
            Values = [new(10, 30)],
            YToolTipLabelFormatter = p =>
                $"from {p.Coordinate.TertiaryValue:N0} to {p.Coordinate.PrimaryValue:N0}",
        };
        var chart = new SKCartesianChart
        {
            Width = 200,
            Height = 200,
            Series = [series],
            YAxes = [new Axis { Labeler = v => $"${v}", MinLimit = 0, MaxLimit = 50 }], // would say "$10 → $30" by default
        };

        Measure(chart);

        var point = series.everFetched.Single();
        var tooltip = series.GetPrimaryToolTipText(point);

        Assert.AreEqual("from 10 to 30", tooltip);
    }

    [TestMethod]
    public void RangeRow_DateTimeAxis_RendersDatesInTooltip()
    {
        // Gantt-style: horizontal bars on a DateTime X axis. The "value" axis
        // for a row series is X, not Y — the override must read XAxes[ScalesXAt],
        // not YAxes[...].
        var start = new DateTime(2026, 6, 1);
        var end = new DateTime(2026, 6, 5);
        var series = new RangeRowSeries<RangeValue>
        {
            Values = [new(start.Ticks, end.Ticks)],
        };
        var chart = new SKCartesianChart
        {
            Width = 200,
            Height = 200,
            Series = [series],
            XAxes = [new DateTimeAxis(TimeSpan.FromDays(1), d => d.ToString("MMM dd", CultureInfo.InvariantCulture))],
        };

        Measure(chart);

        var point = series.everFetched.Single();
        var tooltip = series.GetPrimaryToolTipText(point);

        Assert.AreEqual("Jun 01 → Jun 05", tooltip);
    }

    [TestMethod]
    public void RangeRow_UserFormatter_TakesPrecedence()
    {
        // Row series follow the column-series convention: YToolTipLabelFormatter is the
        // "primary/value" formatter (body of the tooltip), regardless of the orientation
        // of the value axis. XToolTipLabelFormatter feeds the cross-axis header — see
        // RangeRow_HeaderUsesCategoryLabel_OrSuppressesWhenAbsent.
        var series = new RangeRowSeries<RangeValue>
        {
            Values = [new(10, 30)],
            YToolTipLabelFormatter = p =>
                $"between {p.Coordinate.TertiaryValue} and {p.Coordinate.PrimaryValue}",
        };
        var chart = new SKCartesianChart
        {
            Width = 200,
            Height = 200,
            Series = [series],
            XAxes = [new Axis { Labeler = v => $"${v}", MinLimit = 0, MaxLimit = 50 }],
        };

        Measure(chart);

        var point = series.everFetched.Single();
        var tooltip = series.GetPrimaryToolTipText(point);

        Assert.AreEqual("between 10 and 30", tooltip);
    }

    [TestMethod]
    public void RangeRow_HeaderUsesCategoryLabel_OrSuppressesWhenAbsent()
    {
        // For a Gantt-style row series the header (GetSecondaryToolTipText) should
        // pull the task name from the Y axis Labels — not interpret the entity index
        // through the X axis labeler, which would produce gibberish under a
        // DateTimeAxis.
        var series = new RangeRowSeries<RangeValue>
        {
            Values =
            [
                new(10, 20),
                new(15, 25),
                new(20, 30),
            ],
        };
        var chart = new SKCartesianChart
        {
            Width = 200,
            Height = 200,
            Series = [series],
            XAxes = [new DateTimeAxis(TimeSpan.FromDays(1), d => d.ToString("MMM dd", CultureInfo.InvariantCulture))],
            YAxes = [new Axis { Labels = ["Design", "Backend", "Frontend"] }],
        };

        Measure(chart);

        var points = series.everFetched.OrderBy(p => p.Coordinate.SecondaryValue).ToArray();
        Assert.AreEqual("Design", series.GetSecondaryToolTipText(points[0]));
        Assert.AreEqual("Backend", series.GetSecondaryToolTipText(points[1]));
        Assert.AreEqual("Frontend", series.GetSecondaryToolTipText(points[2]));
    }

    [TestMethod]
    public void RangeRow_Header_SuppressedWhenYAxisHasNoLabels()
    {
        var series = new RangeRowSeries<RangeValue>
        {
            Values = [new(10, 20)],
        };
        var chart = new SKCartesianChart
        {
            Width = 200,
            Height = 200,
            Series = [series],
            XAxes = [new Axis { MinLimit = 0, MaxLimit = 30 }],
            YAxes = [new Axis()],  // no Labels => header should suppress
        };

        Measure(chart);

        var point = series.everFetched.Single();
        Assert.AreEqual(LiveCharts.IgnoreToolTipLabel, series.GetSecondaryToolTipText(point));
    }

    [TestMethod]
    public void RangeRow_Header_XFormatterStillOverrides()
    {
        var series = new RangeRowSeries<RangeValue>
        {
            Values = [new(10, 20)],
            XToolTipLabelFormatter = _ => "custom header",
        };
        var chart = new SKCartesianChart
        {
            Width = 200,
            Height = 200,
            Series = [series],
            XAxes = [new Axis { MinLimit = 0, MaxLimit = 30 }],
            YAxes = [new Axis { Labels = ["A"] }],
        };

        Measure(chart);

        var point = series.everFetched.Single();
        Assert.AreEqual("custom header", series.GetSecondaryToolTipText(point));
    }
}
