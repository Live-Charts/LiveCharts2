using System;
using System.Collections.ObjectModel;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.OtherTests;

// Regression for https://github.com/Live-Charts/LiveCharts2/issues/1926.
// Before the fix, every visual baked in a per-call Animation instance at creation time, so
// changing the chart's AnimationsSpeed at runtime did not reach existing visuals — the user
// had to recreate the series. The fix shares a single Animation reference per series/axis/chart
// and mutates it in place, so the change propagates without recreation.
[TestClass]
public class AnimationsSpeedPropagationTests
{
    [TestMethod]
    public void ColumnSeries_VisualMotion_PicksUpChartLevelAnimationsSpeed()
    {
        var initialSpeed = TimeSpan.FromMilliseconds(123);
        var newSpeed = TimeSpan.FromMilliseconds(456);

        var series = new ColumnSeries<double> { Values = [1d, 2d, 3d] };

        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = initialSpeed,
            Series = [series],
        };

        _ = chart.GetImage();

        var firstPoint = series.everFetched.First();
        var visual = (RoundedRectangleGeometry?)firstPoint.Context.Visual
            ?? throw new InvalidOperationException("expected RoundedRectangleGeometry visual");

        var xMotion = DrawnGeometry.XProperty.GetMotion(visual)!;

        Assert.AreEqual((long)initialSpeed.TotalMilliseconds, xMotion.Animation!.Duration,
            "initial duration should match chart-level AnimationsSpeed.");

        chart.AnimationsSpeed = newSpeed;
        _ = chart.GetImage();

        Assert.AreEqual((long)newSpeed.TotalMilliseconds, xMotion.Animation!.Duration,
            "duration on the existing visual should follow chart-level AnimationsSpeed changes " +
            "without recreating the series.");
    }

    [TestMethod]
    public void Axis_LabelMotion_PicksUpChartLevelAnimationsSpeed()
    {
        var initialSpeed = TimeSpan.FromMilliseconds(150);
        var newSpeed = TimeSpan.FromMilliseconds(750);

        var x = new Axis();

        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = initialSpeed,
            Series = [new LineSeries<double> { Values = [1d, 2d, 3d] }],
            XAxes = [x],
        };

        _ = chart.GetImage();

        var label = x.activeSeparators[chart.CoreChart].Values
            .Select(s => (s as LiveChartsCore.Kernel.Helpers.AxisVisualSeprator)?.Label)
            .First(l => l is not null)!;

        var labelXMotion = BaseLabelGeometry.XProperty.GetMotion(label)!;

        Assert.AreEqual((long)initialSpeed.TotalMilliseconds, labelXMotion.Animation!.Duration);

        chart.AnimationsSpeed = newSpeed;
        _ = chart.GetImage();

        Assert.AreEqual((long)newSpeed.TotalMilliseconds, labelXMotion.Animation!.Duration,
            "axis-label motion should follow chart AnimationsSpeed changes without rebuild.");
    }

    [TestMethod]
    public void ColumnSeries_VisualMotion_PicksUpRuntimePerSeriesOverride()
    {
        // The chart-level setting is irrelevant here: the series carries a per-series override
        // both before and after, and we mutate the override at runtime. Existing visuals must
        // pick up the new override value without recreation.
        var seriesInitial = TimeSpan.FromMilliseconds(75);
        var seriesNew = TimeSpan.FromMilliseconds(925);

        var series = new ColumnSeries<double>
        {
            Values = [1d, 2d, 3d],
            AnimationsSpeed = seriesInitial,
        };

        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = TimeSpan.FromMilliseconds(300),
            Series = [series],
        };

        _ = chart.GetImage();

        var visual = (RoundedRectangleGeometry?)series.everFetched.First().Context.Visual
            ?? throw new InvalidOperationException("expected RoundedRectangleGeometry visual");

        var xMotion = DrawnGeometry.XProperty.GetMotion(visual)!;

        Assert.AreEqual((long)seriesInitial.TotalMilliseconds, xMotion.Animation!.Duration,
            "initial duration should follow the per-series override, not the chart-level value.");

        series.AnimationsSpeed = seriesNew;
        _ = chart.GetImage();

        Assert.AreEqual((long)seriesNew.TotalMilliseconds, xMotion.Animation!.Duration,
            "mutating Series.AnimationsSpeed at runtime should propagate to existing visuals.");
    }

    // Per-frame behavioral test, modelled after TransitionsTesting.cs. Drives chart Measure
    // manually with controlled time (CoreMotionCanvas.DebugElapsedMilliseconds) to verify that
    // when AnimationsSpeed is changed at runtime, the next data-driven SetMovement uses the new
    // duration AND the geometry interpolates between FromValue and ToValue at the expected rate.
    // A reference-identity test (above) only checks that Animation.Duration was mutated; this
    // confirms that the value the renderer would actually paint follows the new speed.
    [TestMethod]
    public void RuntimeAnimationsSpeedChange_Interpolates_AtNewSpeed()
    {
        var initialSpeed = TimeSpan.FromMilliseconds(100);
        var newSpeed = TimeSpan.FromMilliseconds(500);

        var values = new ObservableCollection<double> { 50d };
        var series = new ColumnSeries<double> { Values = values };

        var chart = new SKCartesianChart
        {
            Width = 400,
            Height = 400,
            AnimationsSpeed = initialSpeed,
            // SK chart constructor sets EasingFunction = null (animations off). Restore a real
            // function so transitions actually interpolate; Lineal makes the math trivial.
            EasingFunction = EasingFunctions.Lineal,
            Series = [series],
            // Fixed Y range so changing the column value moves Y deterministically. With
            // auto-limits a single-value dataset always renders the bar at the same height.
            YAxes = [new Axis { MinLimit = 0, MaxLimit = 200 }],
        };

        var coreChart = (CartesianChartEngine)chart.CoreChart;

        try
        {
            // Frame 0: first measure. Visual is created and animated 0 → pixelForValue50.
            CoreMotionCanvas.DebugElapsedMilliseconds = 0;
            coreChart.IsLoaded = true;
            coreChart._isFirstDraw = true;
            coreChart.Measure();

            var visual = (RoundedRectangleGeometry?)series.everFetched.First().Context.Visual
                ?? throw new InvalidOperationException("expected RoundedRectangleGeometry visual");

            var yMotion = (MotionProperty<float>)DrawnGeometry.YProperty.GetMotion(visual)!;

            // Step past the initial animation so we have a stable starting state. Just advancing
            // time and re-measuring is enough — Measure re-asserts visual.Y, but with the same
            // pixel value, so SetMovement collapses to ToValue.
            CoreMotionCanvas.DebugElapsedMilliseconds = (long)initialSpeed.TotalMilliseconds + 50;
            coreChart.Measure();

            var stableY = visual.Y;
            Assert.AreEqual((long)initialSpeed.TotalMilliseconds, yMotion.Animation!.Duration,
                "before runtime change, the Animation.Duration should still be the initial speed.");

            // Now: change AnimationsSpeed AND change the data. The next measure should record a
            // new SetMovement using the new speed.
            chart.AnimationsSpeed = newSpeed;
            values[0] = 100d;
            var measureT = CoreMotionCanvas.DebugElapsedMilliseconds;
            coreChart.Measure();

            Assert.AreEqual((long)newSpeed.TotalMilliseconds, yMotion.Animation.Duration,
                "after the runtime change, SetMovement should run with the new speed.");

            var fromY = yMotion.FromValue;
            var toY = yMotion.ToValue;
            Assert.AreNotEqual(fromY, toY, "values changed; transition should be non-trivial.");
            Assert.AreEqual(stableY, fromY,
                "the new transition should pick up the previous (stable) Y as its FromValue.");

            // Verify per-frame interpolation against the linear formula.
            void AssertProgress(float p)
            {
                CoreMotionCanvas.DebugElapsedMilliseconds = measureT + (long)(p * newSpeed.TotalMilliseconds);
                var actual = visual.Y;
                var expected = fromY + p * (toY - fromY);
                Assert.IsTrue(Math.Abs(actual - expected) < 0.01f,
                    $"at p={p}, expected Y≈{expected}, got {actual}.");
            }

            AssertProgress(0f);
            AssertProgress(0.25f);
            AssertProgress(0.5f);
            AssertProgress(0.75f);
            AssertProgress(1f);

            // Past the end of the new animation (which is 500ms long), Y should sit at ToValue.
            CoreMotionCanvas.DebugElapsedMilliseconds = measureT + (long)newSpeed.TotalMilliseconds + 50;
            Assert.AreEqual(toY, visual.Y);

            // Sanity check that the OLD speed (100ms) is no longer in play: at measureT + 100ms,
            // a properly-mutated Animation gives p=0.2 — under the bug, p=1.0 (animation already
            // completed because Duration was still 100ms).
            CoreMotionCanvas.DebugElapsedMilliseconds = measureT + (long)initialSpeed.TotalMilliseconds;
            var underNewSpeed = fromY + (initialSpeed.TotalMilliseconds / newSpeed.TotalMilliseconds) * (toY - fromY);
            Assert.IsTrue(Math.Abs(visual.Y - underNewSpeed) < 0.01f,
                "if the bug regressed, Y would equal ToValue here (old 100ms duration); " +
                "with the fix, it should be only ~20% of the way through the new 500ms transition.");
        }
        finally
        {
            CoreMotionCanvas.DebugElapsedMilliseconds = -1;
        }
    }
}
