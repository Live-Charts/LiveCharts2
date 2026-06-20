using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Providers;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.ChartTests;

// Guards the ISeriesRenderOverride provider seam: a provider's override must be
// consulted on all three data paths (render, bounds, hit-test) and on removal, and
// the chart must fall back to the series itself when the override declines.
[TestClass]
public class SeriesRenderOverrideTests
{
    private class RecordingOverride : ISeriesRenderOverride
    {
        public bool RenderConsulted, BoundsConsulted, HitTestConsulted, Removed;

        // Test knob for the engage-cleanup behaviour.
        public bool Engage;                       // TryRender returns this

        // Record then take over / decline per Engage. When declining the chart falls back to the
        // series, so it renders exactly like OSS while we assert the seam was wired.
        public bool TryRender(ISeries series, Chart chart) { RenderConsulted = true; return Engage; }

        public bool TryGetBounds(
            ISeries series, Chart chart, ICartesianAxis secondaryAxis, ICartesianAxis primaryAxis,
            out SeriesBounds bounds)
        {
            BoundsConsulted = true;
            bounds = default;
            return false;
        }

        public IEnumerable<ChartPoint>? TryFindHitPoints(
            ISeries series, Chart chart, LvcPoint pointerPosition, FindingStrategy strategy, FindPointFor findPointFor)
        {
            HitTestConsulted = true;
            return null;
        }

        public void OnRemoved(IChartView view, ISeries series) => Removed = true;
    }

    private sealed class FakeEngine(RecordingOverride over) : SkiaSharpProvider
    {
        // Passthrough SkiaSharp engine that only adds the override (for any series),
        // so configuring it globally for the test doesn't change anything else.
        public override ISeriesRenderOverride? GetRenderOverride(ISeries series) => over;
    }

    [TestMethod]
    public void Override_IsConsultedOnAllPaths_AndChartFallsBack()
    {
        var over = new RecordingOverride();
        try
        {
            LiveCharts.Configure(s => s.HasProvider(new FakeEngine(over)));

            var series = new LineSeries<double> { Values = [1, 2, 3, 4], GeometrySize = 0 };
            var chart = new SKCartesianChart
            {
                Width = 600,
                Height = 400,
                Series = [series],
                XAxes = [new Axis()],
                YAxes = [new Axis()]
            };

            // Measure: rendering (AddVisual) + bounds (Measure) consult the override.
            var image = chart.GetImage();
            Assert.IsNotNull(image, "chart must still render via the fallback when the override declines");
            Assert.IsTrue(over.RenderConsulted, "TryRender must be consulted during measure");
            Assert.IsTrue(over.BoundsConsulted, "TryGetBounds must be consulted during measure");

            // Hit-test path consults the override (enumerate — GetPointsAt is lazy).
            _ = chart.GetPointsAt(new LvcPointD(300, 200), FindingStrategy.CompareAll).ToArray();
            Assert.IsTrue(over.HitTestConsulted, "TryFindHitPoints must be consulted on hit-test");
        }
        finally
        {
            LiveCharts.Configure(s => s.AddSkiaSharp());
        }
    }

    // When an override ENGAGES (TryRender returns true) the chart must bypass the series'
    // per-point Invalidate entirely — the override owns the drawing and any cleanup of the
    // series' own visuals. A fresh chart whose override engages and draws nothing therefore
    // produces NO series paint tasks, whereas the same chart with a declining override renders
    // the series normally. (Axes are stripped of paints so the count reflects series paints only.)
    [TestMethod]
    public void EngagingOverride_BypassesTheSeriesPerPointInvalidate()
    {
        try
        {
            static SKCartesianChart NewChart() => new()
            {
                Width = 600,
                Height = 400,
                Series = [new LineSeries<double> { Values = [1, 2, 3, 4], GeometrySize = 0 }],
                XAxes = [BareAxis()],
                YAxes = [BareAxis()],
            };

            static int Measure(SKCartesianChart chart, RecordingOverride over, bool engage)
            {
                over.Engage = engage;
                var core = chart.CoreChart;
                core.IsLoaded = true;
                core._isFirstDraw = true;
                core.Measure();
                using var surface = SKSurface.Create(new SKImageInfo(10, 10));
                chart.CoreCanvas.DrawFrame(new SkiaSharpDrawingContext(chart.CoreCanvas, surface.Canvas, SKColor.Empty));
                return chart.CoreCanvas.CountPaintTasks();
            }

            // Declining override → the series renders its own paints.
            var declining = new RecordingOverride();
            LiveCharts.Configure(s => s.HasProvider(new FakeEngine(declining)));
            var declined = Measure(NewChart(), declining, engage: false);

            // Engaging override that draws nothing → the series' Invalidate is skipped → no series paints.
            var engaging = new RecordingOverride();
            LiveCharts.Configure(s => s.HasProvider(new FakeEngine(engaging)));
            var engaged = Measure(NewChart(), engaging, engage: true);

            Assert.IsTrue(declined > 0, "a declining override must let the series render its own paints");
            Assert.IsTrue(engaged < declined,
                $"an engaging override must bypass the series' per-point Invalidate (declined={declined}, engaged={engaged})");
        }
        finally
        {
            LiveCharts.Configure(s => s.AddSkiaSharp());
        }
    }

    // Axes stripped of painted parts so CountPaintTasks reflects only the series geometries.
    private static Axis BareAxis() => new()
    {
        LabelsPaint = null,
        SeparatorsPaint = null,
        SubseparatorsPaint = null,
        TicksPaint = null,
        SubticksPaint = null,
    };
}
