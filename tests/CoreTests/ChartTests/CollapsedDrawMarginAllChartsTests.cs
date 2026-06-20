using System;
using System.Linq;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.ChartTests;

// The collapsed-draw-margin freeze (see CollapsedDrawMarginTests for the cartesian case) is shared by
// EVERY chart engine: each one returned from Measure when DrawMarginSize <= 0, leaving the canvas to
// repaint the previous frame's geometry at its old transform. The fix lives on the base Chart
// (HidePlotZones / ResetPlotZoneClips) and is wired into pie, polar, treemap and sankey here, plus
// geo map and cartesian in their own tests. Pie/polar/treemap/sankey never clip their plot zone, so
// "healthy" is DrawMargin clip == Empty (no clip); a collapse clips it to a zero-area non-Empty rect,
// and a valid measure resets it back to Empty.
//
// ExplicitDisposing = true is REQUIRED: without it GetImage calls Unload() after drawing, which cleans
// the canvas zones (Zones = []), and the next read lazily recreates fresh zones whose clips are all
// the default Empty — wiping the very state under test.
[TestClass]
public class CollapsedDrawMarginAllChartsTests
{
    // left + right (and top + bottom) exceed the 300px control, so DrawMarginSize goes negative.
    private static readonly Margin s_collapsing = new(200, 200, 200, 200);

    private static LvcRectangle DrawMarginClip(InMemorySkiaSharpChart chart) =>
        chart.CoreCanvas.Zones[CanvasZone.DrawMargin].Clip;

    private static void AssertHideThenReset(InMemorySkiaSharpChart chart, Action<Margin?> setMargin, string what)
    {
        _ = chart.GetImage();

        // force the draw margin to collapse.
        setMargin(s_collapsing);
        _ = chart.GetImage();

        var collapsed = DrawMarginClip(chart);
        Assert.IsTrue(collapsed.Width <= 0 && collapsed.Height <= 0,
            $"{what}: a collapsed draw margin must clip the plot zone to zero area, got {collapsed.Width}x{collapsed.Height}");
        Assert.AreNotEqual(LvcRectangle.Empty, collapsed,
            $"{what}: the hide-clip must be a constructed zero-area rect, not LvcRectangle.Empty (= no clip)");

        // valid layout again: the plot zone clip must reset to Empty (these engines draw unclipped).
        setMargin(null);
        _ = chart.GetImage();
        Assert.AreEqual(LvcRectangle.Empty, DrawMarginClip(chart),
            $"{what}: a valid measure must reset the plot zone to no-clip after a collapse");
    }

    [TestMethod]
    public void Pie_HidesThenResets()
    {
        var chart = new SKPieChart
        {
            Width = 300,
            Height = 300,
            AnimationsSpeed = TimeSpan.Zero,
            ExplicitDisposing = true,
            Series = [new PieSeries<int> { Values = [1] }, new PieSeries<int> { Values = [2] }],
        };
        AssertHideThenReset(chart, m => chart.DrawMargin = m, "pie");
    }

    // A title taller than the whole control forces the draw margin to collapse on the Top reservation.
    // (Polar ignores the view DrawMargin — a separate, pre-existing bug at PolarChartEngine line 481,
    // where SetDrawMargin uses the raw auto margin `m` instead of the computed `actualMargin` — so we
    // collapse it through the title path that every engine honors.)
    private static LiveChartsCore.SkiaSharpView.VisualElements.DrawnLabelVisual GiantTitle() =>
        new(new LiveChartsCore.SkiaSharpView.Drawing.Geometries.LabelGeometry
        {
            Text = "x",
            TextSize = 4000,
            Paint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(),
        });

    [TestMethod]
    public void Polar_HidesThenResets()
    {
        var chart = new SKPolarChart
        {
            Width = 300,
            Height = 300,
            AnimationsSpeed = TimeSpan.Zero,
            ExplicitDisposing = true,
            Title = GiantTitle(),
            Series = [new PolarLineSeries<int> { Values = [1, 2, 3] }],
        };

        _ = chart.GetImage();
        var collapsed = DrawMarginClip(chart);
        Assert.IsTrue(collapsed.Width <= 0 || collapsed.Height <= 0,
            $"polar: a title taller than the control must collapse the draw margin, got {collapsed.Width}x{collapsed.Height}");
        Assert.AreNotEqual(LvcRectangle.Empty, collapsed,
            "polar: the hide-clip must be a constructed zero-area rect, not LvcRectangle.Empty (= no clip)");

        chart.Title = null;
        _ = chart.GetImage();
        Assert.AreEqual(LvcRectangle.Empty, DrawMarginClip(chart),
            "polar: a valid measure must reset the plot zone to no-clip after a collapse");
    }

    [TestMethod]
    public void Treemap_HidesThenResets()
    {
        var chart = new SKTreemapChart
        {
            Width = 300,
            Height = 300,
            AnimationsSpeed = TimeSpan.Zero,
            ExplicitDisposing = true,
            Series = [new TreemapSeries<TreemapNode> { Values = [new(1), new(2), new(3)] }],
        };
        AssertHideThenReset(chart, m => chart.DrawMargin = m, "treemap");
    }

    [TestMethod]
    public void Sankey_HidesThenResets()
    {
        var sources = new[] { new SankeyNode("A"), new SankeyNode("B") };
        var sinks = new[] { new SankeyNode("X"), new SankeyNode("Y") };
        var chart = new SKSankeyChart
        {
            Width = 300,
            Height = 300,
            AnimationsSpeed = TimeSpan.Zero,
            ExplicitDisposing = true,
            Series =
            [
                new SankeySeries<SankeyNode>
                {
                    Values = sources.Concat(sinks).ToArray(),
                    Links =
                    [
                        new SankeyLink<SankeyNode>(sources[0], sinks[0], 4),
                        new SankeyLink<SankeyNode>(sources[1], sinks[1], 6),
                    ],
                }
            ],
        };
        AssertHideThenReset(chart, m => chart.DrawMargin = m, "sankey");
    }

    // Geo map already had a collapse branch but only Invalidate()'d — it left the previous map painted.
    // It now hides too. Unlike the others, geo map re-sets a REAL draw-margin clip on the valid path
    // (its projection rectangle), so recovery restores a positive clip rather than Empty.
    [TestMethod]
    public void GeoMap_HidesThenResets()
    {
        var chart = new SKGeoMap
        {
            Width = 300,
            Height = 300,
            ExplicitDisposing = true,
            Title = GiantTitle(),
            Series = [new HeatLandSeries { Lands = [new() { Name = "fra", Value = 10 }] }],
        };

        _ = chart.GetImage();
        var collapsed = DrawMarginClip(chart);
        Assert.IsTrue(collapsed.Width <= 0 && collapsed.Height <= 0,
            $"geomap: a title taller than the control must collapse + hide the plot zone, got {collapsed.Width}x{collapsed.Height}");
        Assert.AreNotEqual(LvcRectangle.Empty, collapsed,
            "geomap: the hide-clip must be a constructed zero-area rect, not LvcRectangle.Empty (= no clip)");

        chart.Title = null;
        _ = chart.GetImage();
        var restored = DrawMarginClip(chart);
        Assert.IsTrue(restored.Width > 0 && restored.Height > 0,
            $"geomap: a valid measure must restore a real positive draw-margin clip, got {restored.Width}x{restored.Height}");
    }
}
