using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.VisualElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class ChartPointerEventsTests
{
    [TestMethod]
    public void SeriesDataPointerDownFiresWithTypedHitPoints()
    {
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewChart(series);
        IEnumerable<ChartPoint<int, RoundedRectangleGeometry, LabelGeometry>>? received = null;
        IChartView? sender = null;
        series.DataPointerDown += (c, pts) => { sender = c; received = pts; };

        _ = ChangingPaintTasks.DrawChart(chart);
        chart.CoreChart.InvokePointerDown(FirstPointCenter(series, chart), isSecondaryAction: false);

        Assert.IsNotNull(received);
        Assert.IsTrue(received!.Any(), "expected at least one typed hit point on the series event");
        Assert.AreSame(chart, sender);
    }

    [TestMethod]
    public void SeriesChartPointPointerDownFiresWithClosestTypedPoint()
    {
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewChart(series);
        ChartPoint<int, RoundedRectangleGeometry, LabelGeometry>? closest = null;
        series.ChartPointPointerDown += (_, p) => closest = p;

        _ = ChangingPaintTasks.DrawChart(chart);
        chart.CoreChart.InvokePointerDown(FirstPointCenter(series, chart), isSecondaryAction: false);

        Assert.IsNotNull(closest);
    }

    [TestMethod]
    public void ChartDataPointerDownFiresWithHitPoints()
    {
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewChart(series);
        IEnumerable<ChartPoint>? received = null;
        IChartView? sender = null;
        chart.DataPointerDown += (c, pts) => { sender = c; received = pts; };

        _ = ChangingPaintTasks.DrawChart(chart);
        chart.CoreChart.InvokePointerDown(FirstPointCenter(series, chart), isSecondaryAction: false);

        Assert.IsNotNull(received);
        Assert.IsTrue(received!.Any(), "expected at least one hit point on the chart event");
        Assert.AreSame(chart, sender);
    }

    [TestMethod]
    public void ChartPointPointerDownFiresWithClosestPoint()
    {
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewChart(series);
        ChartPoint? closest = null;
        chart.ChartPointPointerDown += (_, p) => closest = p;

        _ = ChangingPaintTasks.DrawChart(chart);
        chart.CoreChart.InvokePointerDown(FirstPointCenter(series, chart), isSecondaryAction: false);

        Assert.IsNotNull(closest);
    }

    [TestMethod]
    public void ChartDataPointerDownFiresWithEmptyEnumerationWhenNothingHit()
    {
        // The chart-level event must still fire on an empty-area press so consumers
        // can use it as a "press anywhere" hook.
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewChart(series);
        var fired = false;
        IEnumerable<ChartPoint>? received = null;
        chart.DataPointerDown += (_, pts) => { fired = true; received = pts; };

        _ = ChangingPaintTasks.DrawChart(chart);
        chart.CoreChart.InvokePointerDown(new LvcPoint(-50, -50), isSecondaryAction: false);

        Assert.IsTrue(fired);
        Assert.IsNotNull(received);
        Assert.IsFalse(received!.Any());
    }

    [TestMethod]
    public void ChartPointPointerDownFiresWithNullWhenNothingHit()
    {
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewChart(series);
        var fired = false;
        ChartPoint? received = null;
        chart.ChartPointPointerDown += (_, p) =>
        {
            fired = true;
            received = p;
        };

        _ = ChangingPaintTasks.DrawChart(chart);
        chart.CoreChart.InvokePointerDown(new LvcPoint(-50, -50), isSecondaryAction: false);

        Assert.IsTrue(fired);
        Assert.IsNull(received);
    }

    [TestMethod]
    public void VisualElementsPointerDownFiresOnHitVisual()
    {
        var visual = new GeometryVisual<RectangleGeometry>
        {
            X = 200,
            Y = 200,
            Width = 100,
            Height = 100,
            LocationUnit = MeasureUnit.Pixels,
            SizeUnit = MeasureUnit.Pixels,
        };

        var chart = NewChart(new ColumnSeries<int> { Values = [10, 20, 30] });
        chart.VisualElements = [visual];

        VisualElementsEventArgs? args = null;
        IChartView? sender = null;
        chart.VisualElementsPointerDown += (c, a) => { sender = c; args = a; };

        _ = ChangingPaintTasks.DrawChart(chart);
        chart.CoreChart.InvokePointerDown(new LvcPoint(250, 250), isSecondaryAction: false);

        Assert.IsNotNull(args);
        Assert.IsTrue(args!.VisualElements.Any(), "expected the GeometryVisual to be a hit element");
        Assert.AreSame(chart, sender);
    }

    [TestMethod]
    public void VisualPointerDownFiresOnTheHitVisualInstance()
    {
        var visual = new GeometryVisual<RectangleGeometry>
        {
            X = 200,
            Y = 200,
            Width = 100,
            Height = 100,
            LocationUnit = MeasureUnit.Pixels,
            SizeUnit = MeasureUnit.Pixels,
        };

        var chart = NewChart(new ColumnSeries<int> { Values = [10, 20, 30] });
        chart.VisualElements = [visual];

        IInteractable? raisedBy = null;
        visual.PointerDown += (v, _) => raisedBy = v;

        _ = ChangingPaintTasks.DrawChart(chart);
        chart.CoreChart.InvokePointerDown(new LvcPoint(250, 250), isSecondaryAction: false);

        Assert.AreSame(visual, raisedBy);
    }

    [TestMethod]
    public void HoveredPointsChangedFiresOnPointerLeft()
    {
        // InvokePointerLeft synchronously fires HoveredPointsChanged with
        // (null, <active points>) regardless of prior hover activity — it is the
        // chart's "release any tooltip state" hook.
        var chart = NewChart(new ColumnSeries<int> { Values = [10, 20, 30] });
        var fired = false;
        IEnumerable<ChartPoint>? newPoints = null;
        IEnumerable<ChartPoint>? oldPoints = null;
        chart.HoveredPointsChanged += (_, n, o) =>
        {
            fired = true;
            newPoints = n;
            oldPoints = o;
        };

        _ = ChangingPaintTasks.DrawChart(chart);
        chart.CoreChart.InvokePointerLeft();

        Assert.IsTrue(fired);
        Assert.IsNull(newPoints);
        Assert.IsNotNull(oldPoints);
    }

    private static SKCartesianChart NewChart(ISeries series) => new()
    {
        Series = [series],
        XAxes = [new Axis()],
        YAxes = [new Axis()],
    };

    private static LvcPoint FirstPointCenter(ISeries series, SKCartesianChart chart)
    {
        // After Measure each ChartPoint carries a HoverArea. ColumnSeries lays
        // out a RectangleHoverArea per point; click in its centre so the
        // FindHitPoints query reliably returns the point under test.
        var first = series.Fetch(chart.CoreChart).First();
        var area = (RectangleHoverArea)first.Context.HoverArea!;
        return new LvcPoint(area.X + area.Width / 2, area.Y + area.Height / 2);
    }
}
