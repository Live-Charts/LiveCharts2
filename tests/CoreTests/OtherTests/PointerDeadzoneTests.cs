using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

[TestClass]
public class PointerDeadzoneTests
{
    private static (SKCartesianChart chart, CartesianChartEngine core) CreatePinnedChart(
        ZoomAndPanMode zoomMode = ZoomAndPanMode.None)
    {
        // MinLimit/MaxLimit are pinned so Zoom/Pan operations leave a measurable change
        // on the axis limits without the auto-fit logic snapping things back.
        var xAxis = new Axis { MinLimit = 0, MaxLimit = 10 };
        var yAxis = new Axis { MinLimit = 0, MaxLimit = 100 };

        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            ZoomMode = zoomMode,
            XAxes = [xAxis],
            YAxes = [yAxis],
            Series = [new LineSeries<double> { Values = [1d, 2d, 3d, 4d, 5d, 6d, 7d, 8d, 9d, 10d] }]
        };

        using var image = chart.GetImage();
        var core = (CartesianChartEngine)chart.CoreChart;
        return (chart, core);
    }

    [TestMethod]
    public void PointerDown_AloneDoesNotEngagePan()
    {
        // Issue #1957 deadzone: a press without movement must not flip the chart
        // into panning mode — otherwise every touch on mobile races the tooltip
        // throttler against the pan throttler from the very first frame.
        var (_, core) = CreatePinnedChart(ZoomAndPanMode.Both);

        core.InvokePointerDown(new LvcPoint(100, 100), isSecondaryAction: false);

        Assert.IsFalse(
            core._isPanning,
            "PointerDown alone must not engage pan; pan should engage only after the deadzone is crossed.");
    }

    [TestMethod]
    public void PointerMove_BelowDeadzoneKeepsTooltipMode()
    {
        // Press + tiny jitter (under the 5px threshold) must stay in tooltip mode.
        var (_, core) = CreatePinnedChart(ZoomAndPanMode.Both);

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
        var (_, core) = CreatePinnedChart(ZoomAndPanMode.Both);

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
        var (_, core) = CreatePinnedChart(ZoomAndPanMode.Both);

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
        var (_, core) = CreatePinnedChart(ZoomAndPanMode.None);

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
        var (_, core) = CreatePinnedChart(ZoomAndPanMode.ZoomX | ZoomAndPanMode.ZoomY);

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
        var (_, core) = CreatePinnedChart(ZoomAndPanMode.Both);
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
        var (_, core) = CreatePinnedChart(ZoomAndPanMode.Both);
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
        var (_, core) = CreatePinnedChart(ZoomAndPanMode.Both);

        core.InvokePointerDown(new LvcPoint(123, 45), isSecondaryAction: false);

        Assert.AreEqual(123f, core._pointerPosition.X, "PointerDown must seed _pointerPosition.X to the press point.");
        Assert.AreEqual(45f, core._pointerPosition.Y, "PointerDown must seed _pointerPosition.Y to the press point.");
        Assert.IsTrue(core._isPointerIn, "PointerDown must set _isPointerIn so DrawToolTip can paint on a static tap.");
    }
}
