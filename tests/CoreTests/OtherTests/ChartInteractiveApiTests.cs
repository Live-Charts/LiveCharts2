using System;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Geo;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

[TestClass]
public class ChartInteractiveApiTests
{
    [TestMethod]
    public void CartesianZoomIn_And_Pan_AdjustAxisLimits()
    {
        var xAxis = new Axis();
        var yAxis = new Axis();

        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            XAxes = [xAxis],
            YAxes = [yAxis],
            Series = [ new LineSeries<double> { Values = [1d, 2d, 3d, 4d, 5d] } ]
        };

        _ = chart.GetImage();

        var core = (CartesianChartEngine)chart.CoreChart;

        // Zoom in at the center — exercises the "DefinedByZoomIn" path.
        core.Zoom(
            ZoomAndPanMode.Both,
            new LvcPoint(200, 200),
            ZoomDirection.ZoomIn);

        // Pan by a delta — exercises PanAxis on both axes.
        core.Pan(ZoomAndPanMode.Both, new LvcPoint(20, 20));

        _ = chart.GetImage();
        // A second render should not throw.
    }

    [TestMethod]
    public void CartesianZoomWithScaleFactor_WithWrongDirection_Throws()
    {
        var chart = new SKCartesianChart
        {
            Width = 200,
            Height = 200,
            Series = [ new LineSeries<double> { Values = [1d, 2d, 3d] } ]
        };
        _ = chart.GetImage();

        var core = (CartesianChartEngine)chart.CoreChart;

        // When a scale factor is provided the direction MUST be DefinedByScaleFactor.
        Assert.ThrowsExactly<InvalidOperationException>(
            () => core.Zoom(
                ZoomAndPanMode.X,
                new LvcPoint(0, 0),
                ZoomDirection.ZoomIn,
                scaleFactor: 1.5));
    }

    [TestMethod]
    public void CartesianZoomingSection_StartGrowEndFullCycle()
    {
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = [ new LineSeries<double> { Values = [1d, 2d, 3d, 4d] } ]
        };
        _ = chart.GetImage();

        var core = (CartesianChartEngine)chart.CoreChart;

        // Simulate the user dragging a zoom box across the draw margin.
        var start = new LvcPoint(
            core.DrawMarginLocation.X + 20,
            core.DrawMarginLocation.Y + 20);
        var end = new LvcPoint(
            core.DrawMarginLocation.X + core.DrawMarginSize.Width - 20,
            core.DrawMarginLocation.Y + core.DrawMarginSize.Height - 20);

        core.StartZoomingSection(ZoomAndPanMode.Both, start);
        core.GrowZoomingSection(ZoomAndPanMode.Both, new LvcPoint(
            (start.X + end.X) / 2, (start.Y + end.Y) / 2));
        core.EndZoomingSection(ZoomAndPanMode.Both, end);

        _ = chart.GetImage();
    }

    [TestMethod]
    public void CartesianZoomingSection_StartOutsideDrawMarginIsNoOp()
    {
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = [ new LineSeries<double> { Values = [1d, 2d, 3d] } ]
        };
        _ = chart.GetImage();

        var core = (CartesianChartEngine)chart.CoreChart;

        // Starting outside the draw margin should early-return without crashing.
        core.StartZoomingSection(ZoomAndPanMode.Both, new LvcPoint(-10, -10));
        _ = chart.GetImage();
    }

    [TestMethod]
    public void CartesianZoomingSection_NoZoomBySectionFlagIsNoOp()
    {
        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            Series = [ new LineSeries<double> { Values = [1d, 2d, 3d] } ]
        };
        _ = chart.GetImage();

        var core = (CartesianChartEngine)chart.CoreChart;

        core.StartZoomingSection(
            ZoomAndPanMode.Both | ZoomAndPanMode.NoZoomBySection,
            new LvcPoint(100, 100));
    }

    private static (SKCartesianChart chart, Axis xAxis, Axis yAxis, CartesianChartEngine core) CreatePinnedChart(
        ZoomAndPanMode zoomMode = ZoomAndPanMode.None)
    {
        // MinLimit/MaxLimit are pinned so Zoom/Pan operations leave a measurable change
        // on the axis limits without the auto-fit logic snapping things back.
        var xAxis = new Axis { MinLimit = 0, MaxLimit = 10 };
        var yAxis = new Axis { MinLimit = 0, MaxLimit = 10 };

        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            ZoomMode = zoomMode,
            XAxes = [xAxis],
            YAxes = [yAxis],
            Series = [ new LineSeries<double> { Values = [1d, 2d, 3d, 4d, 5d, 6d, 7d, 8d, 9d, 10d] } ]
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
            Series = [ new LineSeries<double> { Values = [1d, 2d, 3d, 4d, 5d, 6d, 7d, 8d, 9d, 10d] } ]
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

    [TestMethod]
    public void PointerDown_AloneDoesNotEngagePan()
    {
        // Issue #1957 deadzone: a press without movement must not flip the chart
        // into panning mode — otherwise every touch on mobile races the tooltip
        // throttler against the pan throttler from the very first frame.
        var (_, _, _, core) = CreatePinnedChart(ZoomAndPanMode.Both);

        core.InvokePointerDown(new LvcPoint(100, 100), isSecondaryAction: false);

        Assert.IsFalse(
            core._isPanning,
            "PointerDown alone must not engage pan; pan should engage only after the deadzone is crossed.");
    }

    [TestMethod]
    public void PointerMove_BelowDeadzoneKeepsTooltipMode()
    {
        // Press + tiny jitter (under the 5px threshold) must stay in tooltip mode.
        var (_, _, _, core) = CreatePinnedChart(ZoomAndPanMode.Both);

        core.InvokePointerDown(new LvcPoint(100, 100), isSecondaryAction: false);
        core.InvokePointerMove(new LvcPoint(102, 101)); // sqrt(5) ≈ 2.2 px

        Assert.IsFalse(
            core._isPanning,
            "Movement under the deadzone must not engage pan.");
    }

    [TestMethod]
    public void PointerMove_AboveDeadzoneEngagesPan()
    {
        // Press + meaningful drag (over the 5px threshold) must engage pan.
        var (_, _, _, core) = CreatePinnedChart(ZoomAndPanMode.Both);

        core.InvokePointerDown(new LvcPoint(100, 100), isSecondaryAction: false);
        core.InvokePointerMove(new LvcPoint(120, 120)); // 28 px diagonal

        Assert.IsTrue(
            core._isPanning,
            "Movement past the deadzone must engage pan.");
    }

    [TestMethod]
    public void PointerUp_ResetsPanState()
    {
        // After release, _isPanning must clear so a subsequent gesture starts
        // from tooltip mode again.
        var (_, _, _, core) = CreatePinnedChart(ZoomAndPanMode.Both);

        core.InvokePointerDown(new LvcPoint(100, 100), isSecondaryAction: false);
        core.InvokePointerMove(new LvcPoint(120, 120)); // engages pan
        core.InvokePointerUp(new LvcPoint(120, 120), isSecondaryAction: false);

        Assert.IsFalse(core._isPanning, "PointerUp must clear _isPanning.");
        Assert.IsFalse(core._isPointerDown, "PointerUp must clear _isPointerDown.");
    }

    [TestMethod]
    public void PointerMove_AboveDeadzone_DoesNotEngagePanWhenZoomModeIsNone()
    {
        // Regression for the IsPanEnabled gate: with ZoomMode=None (or any mode
        // without PanX/PanY), the deadzone must NOT engage on a >5px drag —
        // otherwise the tooltip throttler would be silently suppressed on
        // charts that can't pan. Affects Pie/Polar charts and Cartesian charts
        // configured for zoom-only or no-interaction.
        var (_, _, _, core) = CreatePinnedChart(ZoomAndPanMode.None);

        core.InvokePointerDown(new LvcPoint(100, 100), isSecondaryAction: false);
        core.InvokePointerMove(new LvcPoint(120, 120)); // would engage pan if gate were missing

        Assert.IsFalse(
            core._isPanning,
            "Deadzone must not engage when ZoomMode lacks PanX/PanY; otherwise tooltips would be suppressed on non-pannable charts.");
    }

    [TestMethod]
    public void PointerMove_AboveDeadzone_DoesNotEngagePanWhenZoomModeIsZoomOnly()
    {
        // Zoom-only ZoomMode (e.g. ZoomX) must also keep the deadzone gated off:
        // panning is not enabled, so the tooltip path must keep working during
        // a drag.
        var (_, _, _, core) = CreatePinnedChart(ZoomAndPanMode.ZoomX | ZoomAndPanMode.ZoomY);

        core.InvokePointerDown(new LvcPoint(100, 100), isSecondaryAction: false);
        core.InvokePointerMove(new LvcPoint(120, 120));

        Assert.IsFalse(
            core._isPanning,
            "Deadzone must not engage when ZoomMode has zoom flags but no pan flags.");
    }

#if NET5_0_OR_GREATER
    // _isMobile/_isTooltipCanceled live behind #if NET5_0_OR_GREATER in Chart.cs;
    // net462 has no mobile target so the desktop/mobile tooltip-reset distinction
    // is moot there.
    [TestMethod]
    public void PointerUp_OnDesktop_ClearsTooltipCanceledForHoverAfterPan()
    {
        // After deadzone engagement the tooltip is cancelled to avoid flicker
        // during the pan; on desktop (incl. Mac Catalyst once correctly
        // classified — see Chart.cs _isMobile) PointerUp must reset the flag so
        // the next hover can re-show a tooltip. Without this branch the chart
        // would lose tooltip-on-hover for the rest of its lifetime after the
        // first click+pan.
        var (_, _, _, core) = CreatePinnedChart(ZoomAndPanMode.Both);
        core._isMobile = false;

        core.InvokePointerDown(new LvcPoint(100, 100), isSecondaryAction: false);
        core.InvokePointerMove(new LvcPoint(120, 120));
        core.InvokePointerUp(new LvcPoint(120, 120), isSecondaryAction: false);

        Assert.IsFalse(
            core._isTooltipCanceled,
            "Desktop PointerUp must reset _isTooltipCanceled so post-pan hover can re-show tooltips.");
    }

    [TestMethod]
    public void PointerUp_OnMobile_KeepsTooltipCanceledUntilNextPress()
    {
        // Mobile counterpart: with no hover, the tooltip stays cancelled until
        // the next press resets it via InvokePointerDown. Locks down the
        // deliberately-divergent mobile branch so a future refactor doesn't
        // accidentally unify it with the desktop reset.
        var (_, _, _, core) = CreatePinnedChart(ZoomAndPanMode.Both);
        core._isMobile = true;

        core.InvokePointerDown(new LvcPoint(100, 100), isSecondaryAction: false);
        core.InvokePointerMove(new LvcPoint(120, 120));
        core.InvokePointerUp(new LvcPoint(120, 120), isSecondaryAction: false);

        Assert.IsTrue(
            core._isTooltipCanceled,
            "Mobile PointerUp must keep _isTooltipCanceled set so a stale tooltip can't reappear without a fresh press.");
    }
#endif

    [TestMethod]
    public void PointerDown_SeedsTooltipDrawState()
    {
        // Cross-platform tap-to-tooltip: a press alone must seed _pointerPosition
        // and _isPointerIn so DrawToolTip doesn't bail on an early null-position
        // check, otherwise platforms whose press doesn't emit a synthetic Move
        // (iOS UILongPressGestureRecognizer fires Began -> Ended with no Changed
        // when the finger doesn't move) never open a tooltip on a static tap —
        // a pre-existing inconsistency vs Android, which fires Move alongside
        // Down. Without these seeds the throttler call added at the end of
        // InvokePointerDown runs DrawToolTip on stale (-10, -10) / !_isPointerIn
        // state and renders nothing.
        var (_, _, _, core) = CreatePinnedChart(ZoomAndPanMode.Both);

        core.InvokePointerDown(new LvcPoint(123, 45), isSecondaryAction: false);

        Assert.AreEqual(123f, core._pointerPosition.X, "PointerDown must seed _pointerPosition.X to the press point.");
        Assert.AreEqual(45f, core._pointerPosition.Y, "PointerDown must seed _pointerPosition.Y to the press point.");
        Assert.IsTrue(core._isPointerIn, "PointerDown must set _isPointerIn so DrawToolTip can paint on a static tap.");
    }

    private static SKGeoMap CreateGeoMap(MapProjection projection = MapProjection.Mercator) =>
        new()
        {
            Width = 400,
            Height = 400,
            Series = [ new HeatLandSeries { Lands = [ new() { Name = "bra", Value = 1 } ] } ],
            MapProjection = projection
        };

    [TestMethod]
    public void GeoMap_PanAndZoomAndResetViewportBeforeRender()
    {
        // Interactions must happen before GetImage — SourceGenSKMapChart.DrawOnCanvas
        // unconditionally calls Unload() at the end of every render, which nulls the
        // map factory used by Pan/Zoom/ResetViewport.
        var chart = CreateGeoMap();

        chart.CoreChart.Pan(new LvcPoint(10, 20));
        chart.CoreChart.Zoom(new LvcPoint(100, 100), ZoomDirection.ZoomIn);
        chart.CoreChart.Zoom(new LvcPoint(100, 100), ZoomDirection.ZoomOut);
        chart.CoreChart.ResetViewport();

        _ = chart.GetImage();
    }

    [TestMethod]
    public void GeoMap_ViewToAndRotateToBeforeRender()
    {
        var chart = CreateGeoMap(MapProjection.Orthographic);

        chart.CoreChart.ViewTo(command: null);
        chart.CoreChart.RotateTo(longitude: 10, latitude: 20, durationMs: 0);

        _ = chart.GetImage();
    }

    [TestMethod]
    public void GeoMap_PointerEventsFlowThroughInvokers()
    {
        var chart = CreateGeoMap();

        // Drives InvokePointerDown/Move/Up/Left — these raise events and the factory
        // may react via hover/pan hooks. Must run before GetImage.
        chart.CoreChart.InvokePointerDown(new LvcPoint(50, 50), isSecondaryAction: false);
        chart.CoreChart.InvokePointerMove(new LvcPoint(60, 55));
        chart.CoreChart.InvokePointerMove(new LvcPoint(80, 75));
        chart.CoreChart.InvokePointerUp(new LvcPoint(100, 100), isSecondaryAction: false);
        chart.CoreChart.InvokePointerLeft();

        _ = chart.GetImage();
    }

    [TestMethod]
    public void GeoMap_FindLandAtReturnsNullOutsideAnyLand()
    {
        var chart = CreateGeoMap();

        // FindLandAt far outside any rendered land hits the "no match" branch.
        // Must run before GetImage so the chart hasn't been unloaded yet.
        var hit = chart.CoreChart.FindLandAt(new LvcPoint(-1000, -1000));
        Assert.IsNull(hit);

        _ = chart.GetImage();
    }
}
