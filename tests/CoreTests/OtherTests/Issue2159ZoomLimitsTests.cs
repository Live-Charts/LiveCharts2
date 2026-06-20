// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

[TestClass]
public class Issue2159ZoomLimitsTests
{
    // Issue #2159: user pins XAxis MinLimit=0, MaxLimit=180 on a 10-point
    // series (X data ≈ [0, 9]). Before the fix, the zoom-out clamp + the
    // debounced post-zoom Fit both used DataMin/DataMax as the outer rail,
    // so on the first wheel zoom-out the view collapsed to the data bounds
    // and the user's pinned [0, 180] was permanently lost. From then on
    // every further wheel zoom looked like "can only shrink / zoom-in does
    // nothing" because the view was trapped at data scale.

    private static (Axis xAxis, CartesianChartEngine core) CreateReporterChart()
    {
        var xAxis = new Axis { MinLimit = 0, MaxLimit = 180, MinStep = 10 };
        var yAxis = new Axis { MinLimit = -10, MaxLimit = 110, MinStep = 10 };

        var chart = new SKCartesianChart
        {
            Width = 800,
            Height = 400,
            ZoomMode = ZoomAndPanMode.Both,
            XAxes = [xAxis],
            YAxes = [yAxis],
            Series =
            [
                new LineSeries<double>
                {
                    Values = [10d, 50d, 30d, 80d, 20d, 60d, 40d, 70d, 35d, 55d]
                }
            ]
        };
        _ = chart.GetImage();
        var core = (CartesianChartEngine)chart.CoreChart;
        return (xAxis, core);
    }

    // Same data and viewport as the reporter chart, but with NO pinned
    // MinLimit/MaxLimit. This is the "bounce" baseline: the engine is free
    // to fit the view to the data, and zoom-out must overshoot then snap
    // back. The #2159 fix must not break this for unpinned axes.
    private static (Axis xAxis, CartesianChartEngine core) CreateUnpinnedChart()
    {
        var xAxis = new Axis();
        var yAxis = new Axis();

        var chart = new SKCartesianChart
        {
            Width = 800,
            Height = 400,
            ZoomMode = ZoomAndPanMode.Both,
            XAxes = [xAxis],
            YAxes = [yAxis],
            Series =
            [
                new LineSeries<double>
                {
                    Values = [10d, 50d, 30d, 80d, 20d, 60d, 40d, 70d, 35d, 55d]
                }
            ]
        };
        _ = chart.GetImage();
        var core = (CartesianChartEngine)chart.CoreChart;
        return (xAxis, core);
    }

    [TestMethod]
    public void ZoomOut_OnAxisPinnedWiderThanData_GrowsTheView()
    {
        // From a pinned [0, 180] view the natural expectation is "zoom-out
        // makes the visible range larger." Pre-fix, the outer rail was the
        // data bounds (~[0, 9]) plus BouncingDistance threshold, so a single
        // zoom-out actually SHRANK the visible range — the user's pinned
        // [0, 180] was lost on the first wheel tick.
        var (xAxis, core) = CreateReporterChart();
        var pivot = new LvcPoint(400, 200);

        var rangeBefore = xAxis.MaxLimit!.Value - xAxis.MinLimit!.Value;
        core.Zoom(ZoomAndPanMode.Both, pivot, ZoomDirection.ZoomOut);
        var rangeAfter = xAxis.MaxLimit!.Value - xAxis.MinLimit!.Value;

        Assert.IsTrue(rangeAfter > rangeBefore,
            $"ZoomOut must grow the visible range from a user-pinned view; before={rangeBefore}, after={rangeAfter}.");
    }

    [TestMethod]
    public void ZoomIn_ThenZoomOut_RestoresViewToOrBeyondUserPinning()
    {
        // The basic reversibility users expect: zooming in then zooming out
        // by the same amount should return to (or wider than) the original
        // pinned view. Pre-fix this never came back because zoom-out was
        // trapped at data bounds.
        var (xAxis, core) = CreateReporterChart();
        var pivot = new LvcPoint(400, 200);

        core.Zoom(ZoomAndPanMode.Both, pivot, ZoomDirection.ZoomIn);
        var inMax = xAxis.MaxLimit!.Value;
        Assert.IsTrue(inMax < 180,
            $"ZoomIn from [0, 180] should narrow MaxLimit below 180; got {inMax}.");

        for (var i = 0; i < 6; i++)
            core.Zoom(ZoomAndPanMode.Both, pivot, ZoomDirection.ZoomOut);

        var finalMin = xAxis.MinLimit!.Value;
        var finalMax = xAxis.MaxLimit!.Value;

        Assert.IsTrue(finalMin <= 0 + 1e-6,
            $"Repeated ZoomOut must allow the view to return to the user-pinned MinLimit=0; got Min={finalMin}.");
        Assert.IsTrue(finalMax >= 180 - 1e-6,
            $"Repeated ZoomOut must allow the view to return to the user-pinned MaxLimit=180; got Max={finalMax}.");
    }

    [TestMethod]
    public async Task FitAfterZoom_DoesNotSnapPinnedWiderViewToDataBounds()
    {
        // The post-zoom FitAllOnZoom (300 ms debounced) was the second half
        // of the bug: even when a single ZoomAxis call left the view
        // reasonable, the deferred Fit then snapped MinLimit/MaxLimit down
        // to DataMin/DataMax (the source-of-truth for the second symptom in
        // the report: "MinLimit/MaxLimit have no effect").
        var (xAxis, core) = CreateReporterChart();
        var pivot = new LvcPoint(400, 200);

        core.Zoom(ZoomAndPanMode.Both, pivot, ZoomDirection.ZoomIn);
        await Task.Delay(450); // past the 300 ms zoom-fit debounce

        var minAfter = xAxis.MinLimit!.Value;
        var maxAfter = xAxis.MaxLimit!.Value;

        Assert.IsTrue(maxAfter > minAfter,
            $"FitAllOnZoom must not produce a degenerate range; got X=[{minAfter}, {maxAfter}].");
        // Data X data range is [0, 9]; if Fit snapped to data bounds we'd
        // see MaxLimit ≤ ~10 here. Anything meaningfully wider proves the
        // pinned-outer-rail path is engaged.
        Assert.IsTrue(maxAfter > 20,
            $"FitAllOnZoom must not snap a pinned-wider view to data bounds; got X=[{minAfter}, {maxAfter}].");
    }

    // ----------------------------------------------------------------------
    // Bounce-back regression coverage.
    //
    // PR #2240's first attempt recorded the user pin inside the
    // MinLimit/MaxLimit property setter. But the chart engine's own zoom and
    // pan writes land in that same setter (via SetLimits(notify: true)), so
    // every interaction was misrecorded as a "user pin". For an UNPINNED
    // chart that silently turned the engine's zoomed-out / panned view into
    // the outer rail, and the bounce-back-to-fit stopped working. These tests
    // pin that behaviour down for both zoom and pan.
    // ----------------------------------------------------------------------

    [TestMethod]
    public async Task ZoomOut_OnUnpinnedChart_BouncesBackToFitDataBounds()
    {
        // Unpinned chart: a wheel zoom-out is allowed to overshoot the data
        // bounds by BouncingDistance, then the 300 ms debounced FitAllOnZoom
        // must snap the view back so it fits the data again.
        var (xAxis, core) = CreateUnpinnedChart();
        var pivot = new LvcPoint(400, 200);

        core.Zoom(ZoomAndPanMode.Both, pivot, ZoomDirection.ZoomOut);

        // Right after the zoom (before the debounce) the view has overshot.
        var overshootRange = xAxis.MaxLimit!.Value - xAxis.MinLimit!.Value;

        await Task.Delay(450); // past the 300 ms FitAllOnZoom debounce

        var fittedRange = xAxis.MaxLimit!.Value - xAxis.MinLimit!.Value;

        Assert.IsTrue(fittedRange < overshootRange,
            $"FitAllOnZoom must shrink the overshot view back toward the data " +
            $"bounds; overshoot range={overshootRange}, fitted range={fittedRange}.");

        // Data X spans indices [0, 9] (range 9). After the bounce the view
        // must land near that, not stay stuck at the zoomed-out overshoot.
        Assert.IsTrue(fittedRange < 9 * 1.5,
            $"FitAllOnZoom must land near the ~9 data range, not the zoomed-out " +
            $"range; got fitted range={fittedRange}.");
    }

    [TestMethod]
    public async Task RepeatedZoomOut_OnUnpinnedChart_KeepsBouncingBack()
    {
        // The first attempt's pollution compounded: every wheel tick widened
        // the misrecorded "pin", so the view ran away. Repeated zoom-out on an
        // unpinned chart must keep settling back to the data fit.
        var (xAxis, core) = CreateUnpinnedChart();
        var pivot = new LvcPoint(400, 200);

        for (var i = 0; i < 5; i++)
        {
            core.Zoom(ZoomAndPanMode.Both, pivot, ZoomDirection.ZoomOut);
            await Task.Delay(450);
        }

        var fittedRange = xAxis.MaxLimit!.Value - xAxis.MinLimit!.Value;

        Assert.IsTrue(fittedRange < 9 * 1.5,
            $"Repeated zoom-out must keep bouncing back to the ~9 data range; " +
            $"got fitted range={fittedRange} after 5 ticks.");
    }

    [TestMethod]
    public async Task PanThenZoomOut_OnUnpinnedChart_StillBouncesBack()
    {
        // A pan also writes the axis limits through SetLimits(notify: true).
        // The first attempt let that pan write masquerade as a user pin, so a
        // zoom-out *after* a pan had its bounce-back rail collapsed onto the
        // panned view.
        var (xAxis, core) = CreateUnpinnedChart();
        var pivot = new LvcPoint(400, 200);

        core.Pan(ZoomAndPanMode.Both, new LvcPoint(40, 0));
        core.Zoom(ZoomAndPanMode.Both, pivot, ZoomDirection.ZoomOut);

        var overshootRange = xAxis.MaxLimit!.Value - xAxis.MinLimit!.Value;

        await Task.Delay(450);

        var fittedRange = xAxis.MaxLimit!.Value - xAxis.MinLimit!.Value;

        Assert.IsTrue(fittedRange < overshootRange,
            $"After a pan, zoom-out must still bounce back; overshoot " +
            $"range={overshootRange}, fitted range={fittedRange}.");
        Assert.IsTrue(fittedRange < 9 * 1.5,
            $"After a pan, zoom-out must still fit the ~9 data range; got " +
            $"fitted range={fittedRange}.");
    }

    [TestMethod]
    public void Pan_OnUnpinnedChart_CannotEscapeDataBoundsRail()
    {
        // Panning hard past the data must clamp: the visible window stays
        // anchored within BouncingDistance of the data bounds instead of
        // scrolling into empty space forever.
        var (xAxis, core) = CreateUnpinnedChart();

        for (var i = 0; i < 20; i++)
            core.Pan(ZoomAndPanMode.Both, new LvcPoint(200, 0));

        var min = xAxis.MinLimit!.Value;
        var max = xAxis.MaxLimit!.Value;

        // Data X spans [0, 9]. The clamp keeps the window overlapping the data
        // and no more than one BouncingDistance past DataMax — without it,
        // 20 pans of 200 px would scroll the view far into empty space.
        Assert.IsTrue(min <= 9 && max >= 0,
            $"Pan must keep the data within view; got X=[{min}, {max}].");
        Assert.IsTrue(max < 20,
            $"Pan must clamp to the data-bounds rail; a runaway pan would put " +
            $"MaxLimit far past the ~9 data max, got {max}.");
    }

    // ----------------------------------------------------------------------
    // Shared-axes coverage (PR #2240 review).
    //
    // SharedAxes.Set keeps a group of axes' view limits in sync, and
    // CoreAxis.GetLimits() already aggregates Min/Max/DataMin/DataMax across
    // the SharedWith group. The user pin (UserSetMin/UserSetMax) must be
    // aggregated the same way — otherwise a zoom started from an UNPINNED
    // shared axis falls back to the ~data range as its outer rail, and
    // SetLimits then propagates that collapse onto a pinned sibling.
    // ----------------------------------------------------------------------

    // Two charts whose X axes are a shared group: chart 1's X is pinned
    // wider than the data ([0, 180]), chart 2's X is unpinned. Both charts
    // carry the same 10-point series (X data ≈ [0, 9]).
    private static (Axis pinnedX, CartesianChartEngine unpinnedCore) CreateSharedPinnedAndUnpinnedCharts()
    {
        var pinnedX = new Axis { MinLimit = 0, MaxLimit = 180, MinStep = 10 };
        var unpinnedX = new Axis();

        SharedAxes.Set(pinnedX, unpinnedX);

        var pinnedChart = new SKCartesianChart
        {
            Width = 800,
            Height = 400,
            ZoomMode = ZoomAndPanMode.Both,
            XAxes = [pinnedX],
            YAxes = [new Axis()],
            Series =
            [
                new LineSeries<double>
                {
                    Values = [10d, 50d, 30d, 80d, 20d, 60d, 40d, 70d, 35d, 55d]
                }
            ]
        };
        var unpinnedChart = new SKCartesianChart
        {
            Width = 800,
            Height = 400,
            ZoomMode = ZoomAndPanMode.Both,
            XAxes = [unpinnedX],
            YAxes = [new Axis()],
            Series =
            [
                new LineSeries<double>
                {
                    Values = [10d, 50d, 30d, 80d, 20d, 60d, 40d, 70d, 35d, 55d]
                }
            ]
        };

        _ = pinnedChart.GetImage();
        _ = unpinnedChart.GetImage();

        return (pinnedX, (CartesianChartEngine)unpinnedChart.CoreChart);
    }

    [TestMethod]
    public void ZoomFromUnpinnedSharedAxis_DoesNotCollapsePinnedSibling()
    {
        // The wheel zoom-out happens on the chart whose shared axis is NOT
        // pinned. GetLimits() must still surface the sibling's pin as the
        // outer rail; before the fix it aggregated Min/Max/DataMin/DataMax
        // across SharedWith but not UserSetMin/UserSetMax, so the unpinned
        // axis fell back to the ~9 data range and SetLimits propagated that
        // collapse back onto the pinned sibling.
        var (pinnedX, unpinnedCore) = CreateSharedPinnedAndUnpinnedCharts();
        var pivot = new LvcPoint(400, 200);

        var rangeBefore = pinnedX.MaxLimit!.Value - pinnedX.MinLimit!.Value; // 180

        unpinnedCore.Zoom(ZoomAndPanMode.Both, pivot, ZoomDirection.ZoomOut);

        var minAfter = pinnedX.MinLimit!.Value;
        var maxAfter = pinnedX.MaxLimit!.Value;
        var rangeAfter = maxAfter - minAfter;

        Assert.IsTrue(rangeAfter > rangeBefore,
            $"Zoom-out from an unpinned shared axis must grow — not collapse — the " +
            $"pinned sibling's view; before={rangeBefore}, after={rangeAfter}.");
        // The pin [0, 180] must stay within view: a zoom-out only grows it.
        // Pre-fix the sibling collapsed toward the ~9 data range, leaving
        // MaxLimit far below 180.
        Assert.IsTrue(maxAfter >= 180 - 1e-6 && minAfter <= 0 + 1e-6,
            $"Zoom-out from an unpinned shared axis must keep the pinned sibling's " +
            $"[0, 180] pin within view; got X=[{minAfter}, {maxAfter}].");
    }
}
