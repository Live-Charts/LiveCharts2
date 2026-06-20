using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class ChartHoverEventsTests
{
    [TestMethod]
    public async Task HoveredPointsChangedFiresOnPointerMoveOverPoint()
    {
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewChartWithTooltip(series);
        IEnumerable<ChartPoint>? newPoints = null;
        IEnumerable<ChartPoint>? oldPoints = null;
        chart.HoveredPointsChanged += (_, n, o) => { newPoints = n; oldPoints = o; };

        _ = ChangingPaintTasks.DrawChart(chart);
        chart.CoreChart.InvokePointerMove(FirstPointCenter(series, chart));
        await chart.CoreChart.TooltipThrottlerUnlocked();

        Assert.IsNotNull(newPoints);
        Assert.IsTrue(newPoints!.Any(), "expected at least one new hovered point");
        Assert.IsTrue(oldPoints is null || !oldPoints.Any());
    }

    [TestMethod]
    public async Task SeriesChartPointPointerHoverFiresOnPointerEnter()
    {
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewChartWithTooltip(series);
        ChartPoint<int, RoundedRectangleGeometry, LabelGeometry>? hovered = null;
        series.ChartPointPointerHover += (_, p) => hovered = p;

        _ = ChangingPaintTasks.DrawChart(chart);
        chart.CoreChart.InvokePointerMove(FirstPointCenter(series, chart));
        await chart.CoreChart.TooltipThrottlerUnlocked();

        Assert.IsNotNull(hovered);
    }

    [TestMethod]
    public async Task SeriesChartPointPointerHoverDoesNotRefireWhileStillOverSamePoint()
    {
        // The hover event must be idempotent across repeated tooltip updates
        // at the same position — only a new point should re-fire it.
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewChartWithTooltip(series);
        var count = 0;
        series.ChartPointPointerHover += (_, _) => count++;

        _ = ChangingPaintTasks.DrawChart(chart);
        chart.CoreChart.InvokePointerMove(FirstPointCenter(series, chart));
        await chart.CoreChart.TooltipThrottlerUnlocked();
        await chart.CoreChart.TooltipThrottlerUnlocked();

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public async Task SeriesChartPointPointerHoverLostFiresOnPointerLeft()
    {
        // OnPointerLeft is gated by point.IsPointerOver, which is only flipped
        // by OnPointerEnter when ChartPointPointerHover has a subscriber — both
        // events must be subscribed for the engine to track the over/leave pair.
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewChartWithTooltip(series);
        ChartPoint<int, RoundedRectangleGeometry, LabelGeometry>? lost = null;
        series.ChartPointPointerHover += (_, _) => { };
        series.ChartPointPointerHoverLost += (_, p) => lost = p;

        _ = ChangingPaintTasks.DrawChart(chart);
        chart.CoreChart.InvokePointerMove(FirstPointCenter(series, chart));
        await chart.CoreChart.TooltipThrottlerUnlocked();
        chart.CoreChart.InvokePointerLeft();

        Assert.IsNotNull(lost);
    }

    [TestMethod]
    public async Task HoveredPointsChangedFiresOnPointerLeftWithOldPoints()
    {
        // Sister of the synchronous-leave test in ChartPointerEventsTests: after
        // a real hover has built _activePoints, InvokePointerLeft must surface
        // them as the oldPoints argument. Materialize the args inside the handler:
        // the library hands the live _activePoints HashSet to subscribers and then
        // immediately clears it in CleanHoveredPoints — reading after the call
        // returns is too late.
        var series = new ColumnSeries<int> { Values = [10, 20, 30] };
        var chart = NewChartWithTooltip(series);
        var oldCount = -1;
        IEnumerable<ChartPoint>? newPointsOnLeave = new[] { default(ChartPoint)! };

        _ = ChangingPaintTasks.DrawChart(chart);
        chart.CoreChart.InvokePointerMove(FirstPointCenter(series, chart));
        await chart.CoreChart.TooltipThrottlerUnlocked();

        // subscribe AFTER the hover so we only capture the leave fire.
        chart.HoveredPointsChanged += (_, n, o) =>
        {
            newPointsOnLeave = n;
            oldCount = o?.Count() ?? 0;
        };
        chart.CoreChart.InvokePointerLeft();

        Assert.IsNull(newPointsOnLeave);
        Assert.IsTrue(oldCount > 0, "expected the active hover point to surface as oldPoints");
    }

    private static SKCartesianChart NewChartWithTooltip(ColumnSeries<int> series) => new()
    {
        Series = [series],
        XAxes = [new Axis()],
        YAxes = [new Axis()],
        Tooltip = new SKDefaultTooltip(),
    };

    private static LvcPoint FirstPointCenter(ISeries series, SKCartesianChart chart)
    {
        var first = series.Fetch(chart.CoreChart).First();
        var area = (RectangleHoverArea)first.Context.HoverArea!;
        return new LvcPoint(area.X + area.Width / 2, area.Y + area.Height / 2);
    }
}
