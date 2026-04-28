using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

[TestClass]
public class ChartInteractiveApiTests
{
    private static (SKCartesianChart chart, Axis xAxis, Axis yAxis, CartesianChartEngine core) CreatePinnedChart()
    {
        // MinLimit/MaxLimit are pinned so Zoom/Pan operations leave a measurable change
        // on the axis limits without the auto-fit logic snapping things back.
        var xAxis = new Axis { MinLimit = 0, MaxLimit = 10 };
        var yAxis = new Axis { MinLimit = 0, MaxLimit = 10 };

        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            XAxes = [xAxis],
            YAxes = [yAxis],
            Series = [new LineSeries<double> { Values = [1d, 2d, 3d, 4d, 5d, 6d, 7d, 8d, 9d, 10d] }]
        };

        _ = chart.GetImage();
        var core = (CartesianChartEngine)chart.CoreChart;
        return (chart, xAxis, yAxis, core);
    }

    [TestMethod]
    public void Zoom_WithZoomXOnly_AffectsOnlyXAxis()
    {
        var (_, xAxis, yAxis, core) = CreatePinnedChart();

        var xRangeBefore = xAxis.MaxLimit!.Value - xAxis.MinLimit!.Value;
        var yRangeBefore = yAxis.MaxLimit!.Value - yAxis.MinLimit!.Value;

        core.Zoom(ZoomAndPanMode.ZoomX | ZoomAndPanMode.NoFit, new LvcPoint(200, 200), ZoomDirection.ZoomIn);

        var xRangeAfter = xAxis.MaxLimit!.Value - xAxis.MinLimit!.Value;
        var yRangeAfter = yAxis.MaxLimit!.Value - yAxis.MinLimit!.Value;

        Assert.IsTrue(xRangeAfter < xRangeBefore, "ZoomX should shrink the X axis range.");
        Assert.AreEqual(yRangeBefore, yRangeAfter, 1e-9, "ZoomX must leave the Y axis untouched.");
    }

    [TestMethod]
    public void Zoom_WithPanXOnly_DoesNotZoomXAxis()
    {
        // PanX alone enables panning but NOT zooming; a Zoom call should be a no-op.
        var (_, xAxis, yAxis, core) = CreatePinnedChart();

        var xRangeBefore = xAxis.MaxLimit!.Value - xAxis.MinLimit!.Value;
        var yRangeBefore = yAxis.MaxLimit!.Value - yAxis.MinLimit!.Value;

        core.Zoom(ZoomAndPanMode.PanX | ZoomAndPanMode.NoFit, new LvcPoint(200, 200), ZoomDirection.ZoomIn);

        Assert.AreEqual(xRangeBefore, xAxis.MaxLimit!.Value - xAxis.MinLimit!.Value, 1e-9);
        Assert.AreEqual(yRangeBefore, yAxis.MaxLimit!.Value - yAxis.MinLimit!.Value, 1e-9);
    }

    [TestMethod]
    public void Pan_WithPanXOnly_AffectsOnlyXAxis()
    {
        var (_, xAxis, yAxis, core) = CreatePinnedChart();

        var xMinBefore = xAxis.MinLimit!.Value;
        var yMinBefore = yAxis.MinLimit!.Value;

        core.Pan(ZoomAndPanMode.PanX | ZoomAndPanMode.NoFit, new LvcPoint(20, 20));

        Assert.AreNotEqual(xMinBefore, xAxis.MinLimit!.Value, "PanX should shift the X axis.");
        Assert.AreEqual(yMinBefore, yAxis.MinLimit!.Value, 1e-9, "PanX must leave the Y axis untouched.");
    }

    [TestMethod]
    public void Pan_WithZoomXOnly_DoesNotPanXAxis()
    {
        // ZoomX alone enables zoom but NOT pan; a Pan call should be a no-op.
        var (_, xAxis, yAxis, core) = CreatePinnedChart();

        var xMinBefore = xAxis.MinLimit!.Value;
        var yMinBefore = yAxis.MinLimit!.Value;

        core.Pan(ZoomAndPanMode.ZoomX | ZoomAndPanMode.NoFit, new LvcPoint(20, 20));

        Assert.AreEqual(xMinBefore, xAxis.MinLimit!.Value, 1e-9);
        Assert.AreEqual(yMinBefore, yAxis.MinLimit!.Value, 1e-9);
    }

    [TestMethod]
    public void Zoom_WithZoomYOnly_AffectsOnlyYAxis()
    {
        var (_, xAxis, yAxis, core) = CreatePinnedChart();

        var xRangeBefore = xAxis.MaxLimit!.Value - xAxis.MinLimit!.Value;
        var yRangeBefore = yAxis.MaxLimit!.Value - yAxis.MinLimit!.Value;

        core.Zoom(ZoomAndPanMode.ZoomY | ZoomAndPanMode.NoFit, new LvcPoint(200, 200), ZoomDirection.ZoomIn);

        Assert.AreEqual(xRangeBefore, xAxis.MaxLimit!.Value - xAxis.MinLimit!.Value, 1e-9);
        Assert.IsTrue(yAxis.MaxLimit!.Value - yAxis.MinLimit!.Value < yRangeBefore);
    }

    [TestMethod]
    public void Pan_WithPanYOnly_AffectsOnlyYAxis()
    {
        var (_, xAxis, yAxis, core) = CreatePinnedChart();

        var xMinBefore = xAxis.MinLimit!.Value;
        var yMinBefore = yAxis.MinLimit!.Value;

        core.Pan(ZoomAndPanMode.PanY | ZoomAndPanMode.NoFit, new LvcPoint(20, 20));

        Assert.AreEqual(xMinBefore, xAxis.MinLimit!.Value, 1e-9);
        Assert.AreNotEqual(yMinBefore, yAxis.MinLimit!.Value);
    }

    [TestMethod]
    public void CompositeXFlag_StillEnablesBothPanAndZoomOnX()
    {
        // Backward-compat guarantee: ZoomAndPanMode.X must keep pan+zoom semantics.
        var (_, xAxis, yAxis, core) = CreatePinnedChart();

        var xRangeBefore = xAxis.MaxLimit!.Value - xAxis.MinLimit!.Value;
        var yRangeBefore = yAxis.MaxLimit!.Value - yAxis.MinLimit!.Value;
        var xMinBefore = xAxis.MinLimit!.Value;

        core.Zoom(ZoomAndPanMode.X | ZoomAndPanMode.NoFit, new LvcPoint(200, 200), ZoomDirection.ZoomIn);
        core.Pan(ZoomAndPanMode.X | ZoomAndPanMode.NoFit, new LvcPoint(20, 0));

        Assert.IsTrue(xAxis.MaxLimit!.Value - xAxis.MinLimit!.Value < xRangeBefore);
        Assert.AreNotEqual(xMinBefore, xAxis.MinLimit!.Value);
        Assert.AreEqual(yRangeBefore, yAxis.MaxLimit!.Value - yAxis.MinLimit!.Value, 1e-9);
    }

    [TestMethod]
    public async Task Zoom_NoFitArgumentIsHonored_EvenWhenViewZoomModeOmitsIt()
    {
        // Reproduces issue #2119: a manual core.Zoom(... | NoFit, ...) call must keep
        // the zoomed range. Previously the post-zoom debounced fit checked the view's
        // ZoomMode (None here) instead of the flags argument, snapping limits back.
        var xAxis = new Axis { MinLimit = 50, MaxLimit = 60 };
        var yAxis = new Axis { MinLimit = 50, MaxLimit = 60 };

        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            ZoomMode = ZoomAndPanMode.None,
            XAxes = [xAxis],
            YAxes = [yAxis],
            Series = [new LineSeries<double> { Values = [1d, 2d, 3d, 4d, 5d, 6d, 7d, 8d, 9d, 10d] }]
        };
        _ = chart.GetImage();

        var core = (CartesianChartEngine)chart.CoreChart;

        // Pinned MaxLimit (60) sits above the data bounds (1..10). Without NoFit, the
        // post-zoom Fit would clamp MaxLimit down toward the data max (~10).
        core.Zoom(ZoomAndPanMode.Both | ZoomAndPanMode.NoFit, new LvcPoint(200, 200), ZoomDirection.ZoomIn);

        // Wait past the 300 ms debounce so FitAllOnZoom has had a chance to fire.
        await Task.Delay(450);
        _ = chart.GetImage();

        Assert.IsTrue(xAxis.MaxLimit!.Value > 50, $"NoFit must prevent X MaxLimit snap-to-data; X MaxLimit={xAxis.MaxLimit}.");
        Assert.IsTrue(yAxis.MaxLimit!.Value > 50, $"NoFit must prevent Y MaxLimit snap-to-data; Y MaxLimit={yAxis.MaxLimit}.");
    }
}
