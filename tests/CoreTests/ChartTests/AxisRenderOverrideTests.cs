using System;
using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Providers;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.ChartTests;

// Guards the IAxisRenderOverride provider seam: the engine decides which axes are overridden;
// when the engine returns an override it is consulted on measure and the separators/labeler it
// returns are used; when the engine returns null — or the axis' own BUILT-IN grouping (e.g.
// DateTimeAxis.GroupTimeUnits) already took over — the override is not consulted and the axis
// lays itself out from its own pipeline.
[TestClass]
public class AxisRenderOverrideTests
{
    private sealed class RecordingGrouper : IAxisRenderOverride
    {
        public bool Consulted, LabelerUsed;
        public double SeenMin, SeenMax;

        public bool TryGroup(
            ICartesianAxis axis, Chart chart, double min, double max,
            out IEnumerable<double>? separators, out Func<double, string>? labeler)
        {
            Consulted = true;
            SeenMin = min;
            SeenMax = max;
            separators = [10d, 50d, 90d];                 // a fixed, small set unlike the default
            labeler = _ => { LabelerUsed = true; return "X"; };
            return true;
        }
    }

    // Mirrors how a real engine is expected to gate the seam: by whatever criteria the engine
    // owns — an explicit Target instance here, or a concrete axis type + opt-in flag (the
    // TimeSpanAxis form, which has no built-in grouping yet).
    private sealed class FakeEngine(IAxisRenderOverride grouper) : SkiaSharpProvider
    {
        public ICartesianAxis? Target;
        public bool MatchOptedInAxes;

        public override IAxisRenderOverride? GetAxisRenderOverride(ICartesianAxis axis)
        {
            if (Target is not null) return ReferenceEquals(axis, Target) ? grouper : null;
            if (!MatchOptedInAxes) return null;
            return axis switch
            {
                DateTimeAxis { GroupTimeUnits: true } => grouper,
                TimeSpanAxis { GroupTimeUnits: true } => grouper,
                _ => null
            };
        }
    }

    // Yields its items once, then throws if enumerated a second time — proves CoreAxis materializes the
    // override's separators rather than re-enumerating the same iterator in the size and draw passes.
    private sealed class OnceEnumerable(double[] items) : IEnumerable<double>
    {
        private int _calls;
        public IEnumerator<double> GetEnumerator() => _calls++ == 0
            ? ((IEnumerable<double>)items).GetEnumerator()
            : throw new InvalidOperationException("single-use iterator enumerated twice");
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class SingleUseGrouper : IAxisRenderOverride
    {
        public bool LabelerUsed;
        public bool TryGroup(
            ICartesianAxis axis, Chart chart, double min, double max,
            out IEnumerable<double>? separators, out Func<double, string>? labeler)
        {
            separators = new OnceEnumerable([10d, 50d, 90d]);
            labeler = _ => { LabelerUsed = true; return "X"; };
            return true;
        }
    }

    private static SKCartesianChart NewChart(ICartesianAxis xAxis) => new()
    {
        Width = 600,
        Height = 400,
        Series = [new LineSeries<double> { Values = [0, 100], GeometrySize = 0 }],
        XAxes = [xAxis],
        YAxes = [new Axis()],
    };

    private static DateTimeAxis NewDateTimeAxis(bool groupTimeUnits) =>
        new(TimeSpan.FromTicks(1), date => date.Ticks.ToString())
        {
            GroupTimeUnits = groupTimeUnits,
            MinLimit = 0,
            MaxLimit = 100
        };

    [TestMethod]
    public void MatchedAxis_ConsultsOverride_AndUsesItsLabeler()
    {
        var grouper = new RecordingGrouper();
        try
        {
            var xAxis = new Axis { MinLimit = 0, MaxLimit = 100 };
            LiveCharts.Configure(s => s.HasProvider(new FakeEngine(grouper) { Target = xAxis }));
            _ = NewChart(xAxis).GetImage();

            Assert.IsTrue(grouper.Consulted, "the override must be consulted for the matched axis");
            Assert.IsTrue(grouper.LabelerUsed, "the override's labeler must be used to draw the labels");
            Assert.AreEqual(0d, grouper.SeenMin, 1e-6, "the override must receive the visible min");
            Assert.AreEqual(100d, grouper.SeenMax, 1e-6, "the override must receive the visible max");
        }
        finally
        {
            LiveCharts.Configure(s => s.AddSkiaSharp());
        }
    }

    [TestMethod]
    public void GroupTimeUnits_True_OnTimeSpanAxis_ConsultsOverride()
    {
        // TimeSpanAxis carries the GroupTimeUnits flag but has no built-in grouping yet (a
        // duration tier table is its own design) — the provider seam is how an engine can
        // supply one, so the flag must reach the override.
        var grouper = new RecordingGrouper();
        try
        {
            LiveCharts.Configure(s => s.HasProvider(new FakeEngine(grouper) { MatchOptedInAxes = true }));
            _ = NewChart(new TimeSpanAxis(TimeSpan.FromTicks(1), span => span.Ticks.ToString())
            {
                GroupTimeUnits = true,
                MinLimit = 0,
                MaxLimit = 100
            }).GetImage();

            Assert.IsTrue(grouper.Consulted, "the override must be consulted when GroupTimeUnits is true");
            Assert.IsTrue(grouper.LabelerUsed, "the override's labeler must be used to draw the labels");
        }
        finally
        {
            LiveCharts.Configure(s => s.AddSkiaSharp());
        }
    }

    [TestMethod]
    public void BuiltInGrouping_WinsOverTheProviderOverride()
    {
        // A DateTimeAxis with GroupTimeUnits groups ITSELF (the built-in pipeline); an engine
        // override for the same axis must not be consulted — built-in first, seam second.
        var grouper = new RecordingGrouper();
        try
        {
            LiveCharts.Configure(s => s.HasProvider(new FakeEngine(grouper) { MatchOptedInAxes = true }));

            var labelsPaint = new SolidColorPaint(SKColors.Black);
            var axis = new DateTimeAxis(TimeSpan.FromDays(1), d => d.ToString("d"))
            {
                GroupTimeUnits = true,
                LabelsPaint = labelsPaint,
                MinLimit = new DateTime(2020, 1, 1).Ticks,
                MaxLimit = new DateTime(2021, 12, 31).Ticks,
            };
            var chart = NewChart(axis);
            _ = CoreObjectsTests.ChangingPaintTasks.DrawChart(chart);

            Assert.IsFalse(grouper.Consulted, "the built-in grouping takes the range, so the seam is not consulted");
            Assert.IsTrue(
                labelsPaint.GetGeometries(chart.CoreCanvas).Cast<BaseLabelGeometry>().Any(),
                "the built-in grouping drew the labels");
        }
        finally
        {
            LiveCharts.Configure(s => s.AddSkiaSharp());
        }
    }

    [TestMethod]
    public void GroupedSeparators_SingleUseIterator_AreMaterialized()
    {
        var grouper = new SingleUseGrouper();
        try
        {
            var xAxis = new Axis { MinLimit = 0, MaxLimit = 100 };
            LiveCharts.Configure(s => s.HasProvider(new FakeEngine(grouper) { Target = xAxis }));

            // Re-enumerating a single-use iterator (size pass + draw pass) would throw; materializing
            // it once must not. The labeler running proves the separators reached the draw pass.
            _ = NewChart(xAxis).GetImage();

            Assert.IsTrue(grouper.LabelerUsed, "the grouped labeler must run, so the separators were used in the draw pass");
        }
        finally
        {
            LiveCharts.Configure(s => s.AddSkiaSharp());
        }
    }

    [TestMethod]
    public void GroupTimeUnits_False_DoesNotConsultOverride()
    {
        var grouper = new RecordingGrouper();
        try
        {
            LiveCharts.Configure(s => s.HasProvider(new FakeEngine(grouper) { MatchOptedInAxes = true }));
            _ = NewChart(NewDateTimeAxis(groupTimeUnits: false)).GetImage();

            Assert.IsFalse(grouper.Consulted, "the override must NOT be consulted when GroupTimeUnits is false");
        }
        finally
        {
            LiveCharts.Configure(s => s.AddSkiaSharp());
        }
    }

    [TestMethod]
    public void GroupedLabels_CenterTheirLines_AndResetWhenNotGrouped()
    {
        // Grouped labels are multi-line blocks centered on their tick; left-aligned lines would
        // shift the narrow line toward the previous tick (the "18:0000:00" collision). Ungrouped
        // axes must keep the default Start alignment so existing layouts don't move.
        var grouper = new RecordingGrouper();
        try
        {
            LiveCharts.Configure(s => s.HasProvider(new FakeEngine(grouper) { MatchOptedInAxes = true }));

            var groupedPaint = new SolidColorPaint(SKColors.Black);
            var groupedAxis = NewDateTimeAxis(groupTimeUnits: true);
            groupedAxis.LabelsPaint = groupedPaint;
            var groupedChart = NewChart(groupedAxis);
            _ = CoreObjectsTests.ChangingPaintTasks.DrawChart(groupedChart);

            var groupedLabels = groupedPaint.GetGeometries(groupedChart.CoreCanvas)
                .Cast<BaseLabelGeometry>().ToArray();
            Assert.IsTrue(groupedLabels.Length > 0, "expected drawn axis labels");
            Assert.IsTrue(
                groupedLabels.All(l => l.LinesAlignment == Align.Middle),
                "grouped multi-line labels must center their lines on the tick");

            var plainPaint = new SolidColorPaint(SKColors.Black);
            var plainAxis = NewDateTimeAxis(groupTimeUnits: false);
            plainAxis.LabelsPaint = plainPaint;
            var plainChart = NewChart(plainAxis);
            _ = CoreObjectsTests.ChangingPaintTasks.DrawChart(plainChart);

            var plainLabels = plainPaint.GetGeometries(plainChart.CoreCanvas)
                .Cast<BaseLabelGeometry>().ToArray();
            Assert.IsTrue(plainLabels.Length > 0, "expected drawn axis labels");
            Assert.IsTrue(
                plainLabels.All(l => l.LinesAlignment == Align.Start),
                "ungrouped labels must keep the default Start alignment");
        }
        finally
        {
            LiveCharts.Configure(s => s.AddSkiaSharp());
        }
    }

    [TestMethod]
    public void PlainAxis_DoesNotConsultOverride()
    {
        var grouper = new RecordingGrouper();
        try
        {
            LiveCharts.Configure(s => s.HasProvider(new FakeEngine(grouper) { MatchOptedInAxes = true }));
            _ = NewChart(new Axis { MinLimit = 0, MaxLimit = 100 }).GetImage();

            Assert.IsFalse(grouper.Consulted, "the override must NOT be consulted for axes the engine does not match");
        }
        finally
        {
            LiveCharts.Configure(s => s.AddSkiaSharp());
        }
    }
}
