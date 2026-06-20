using System;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace CoreTests.ChartTests;

// Alternating (zebra) axis bands: setting ICartesianAxis.AlternatingBandsPaint fills every
// other gap between the axis separators with a draw-margin-sized rectangle behind the series.
// Covers the rendering lifecycle (drawn, rasterized, animated out when the paint is removed)
// and the zebra computation — especially parity stability: panning must not swap the band
// colors, so parity is anchored to a stable cell ordinal, not to the separator list index.
[TestClass]
public class AlternatingBandsTests
{
    private static SKCartesianChart NewChart(ICartesianAxis xAxis) => new()
    {
        Width = 600,
        Height = 400,
        Series = [new LineSeries<double> { Values = [0, 100], GeometrySize = 0 }],
        XAxes = [xAxis],
        YAxes = [new Axis()],
    };

    [TestMethod]
    public void Bands_AreDrawn_AsDrawMarginSizedRectangles()
    {
        var paint = new SolidColorPaint(SKColors.LightGray);
        var xAxis = new Axis
        {
            MinLimit = 0,
            MaxLimit = 100,
            CustomSeparators = [0d, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100],
            AlternatingBandsPaint = paint,
        };

        var chart = NewChart(xAxis);
        _ = CoreObjectsTests.ChangingPaintTasks.DrawChart(chart);

        var rects = paint.GetGeometries(chart.CoreCanvas).Cast<BoundedDrawnGeometry>().ToArray();
        Assert.AreEqual(5, rects.Length, "ten cells, every other one banded");

        var drawMarginHeight = chart.CoreChart.DrawMarginSize.Height;
        foreach (var rect in rects)
        {
            Assert.IsTrue(rect.Width > 0, "an X-axis band has the band range as width");
            Assert.AreEqual(drawMarginHeight, rect.Height, 0.5, "an X-axis band spans the draw margin height");
        }

        Assert.AreEqual(rects[0].Width, rects[1].Width, 0.5, "equal ranges produce equal widths");
        Assert.AreNotEqual(rects[0].X, rects[1].X, "bands sit at their own positions");
    }

    [TestMethod]
    public void Bands_Rasterize()
    {
        // The geometries existing is not enough — they must reach pixels (a paint whose style
        // never resolved to Fill would draw nothing, which is exactly how this was first caught
        // when the paint came from an engine instead of the property setter).
        var paint = new SolidColorPaint(SKColors.Red);
        var xAxis = new Axis { MinLimit = 0, MaxLimit = 100, AlternatingBandsPaint = paint };

        var chart = NewChart(xAxis);
        _ = CoreObjectsTests.ChangingPaintTasks.DrawChart(chart);

        using var surface = SKSurface.Create(new SKImageInfo(600, 400));
        surface.Canvas.Clear(SKColors.White);
        chart.CoreCanvas.DrawFrame(
            new LiveChartsCore.SkiaSharpView.Drawing.SkiaSharpDrawingContext(
                chart.CoreCanvas, surface.Canvas, SKColor.Empty));

        using var image = surface.Snapshot();
        using var bitmap = SKBitmap.FromImage(image);
        var red = 0;
        for (var x = 0; x < bitmap.Width; x += 2)
            for (var y = 0; y < bitmap.Height; y += 2)
            {
                var c = bitmap.GetPixel(x, y);
                if (c.Red > 200 && c.Green < 100 && c.Blue < 100) red++;
            }

        Assert.IsTrue(red > 0, "the bands must rasterize");
    }

    [TestMethod]
    public void Bands_Detach_WhenThePaintIsRemoved()
    {
        var paint = new SolidColorPaint(SKColors.LightGray);
        var xAxis = new Axis { MinLimit = 0, MaxLimit = 100, AlternatingBandsPaint = paint };

        var chart = NewChart(xAxis);
        _ = CoreObjectsTests.ChangingPaintTasks.DrawChart(chart);
        Assert.IsTrue(paint.GetGeometries(chart.CoreCanvas).Any(), "bands drawn while the paint is set");

        xAxis.AlternatingBandsPaint = null;
        _ = CoreObjectsTests.ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(
            0, paint.GetGeometries(chart.CoreCanvas).Count(),
            "bands must detach once the paint is removed");
    }

    [TestMethod]
    public void SwappingThePaint_MovesTheBands_AndDropsTheOldTask()
    {
        // Swapping the paint instance (a theme change) must ride the standard paint
        // lifecycle: the old paint's task leaves the canvas (no double-rendered bands, no
        // orphan task) and the cached band geometries re-attach to the new paint.
        var first = new SolidColorPaint(SKColors.LightGray);
        var xAxis = new Axis { MinLimit = 0, MaxLimit = 100, AlternatingBandsPaint = first };

        var chart = NewChart(xAxis);
        _ = CoreObjectsTests.ChangingPaintTasks.DrawChart(chart);
        Assert.IsTrue(first.GetGeometries(chart.CoreCanvas).Any(), "bands drawn on the first paint");

        var second = new SolidColorPaint(SKColors.Gray);
        xAxis.AlternatingBandsPaint = second;
        _ = CoreObjectsTests.ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(
            0, first.GetGeometries(chart.CoreCanvas).Count(),
            "the old paint task must leave the canvas — bands would otherwise render twice");
        Assert.IsTrue(second.GetGeometries(chart.CoreCanvas).Any(), "the bands ride the new paint");
    }

    private sealed class NoBandGeometryAxis : Axis
    {
        protected override LiveChartsCore.Drawing.BoundedDrawnGeometry? CreateBandGeometry() => null;
    }

    [TestMethod]
    public void PlatformWithoutBandGeometry_DisablesBandsCleanly()
    {
        // CreateBandGeometry returning null means "this platform draws no bands": with the
        // paint set, nothing renders and no state is touched — the documented disable
        // contract, exercised through a full measure.
        var paint = new SolidColorPaint(SKColors.LightGray);
        var xAxis = new NoBandGeometryAxis { MinLimit = 0, MaxLimit = 100, AlternatingBandsPaint = paint };

        var chart = NewChart(xAxis);
        _ = CoreObjectsTests.ChangingPaintTasks.DrawChart(chart);

        Assert.AreEqual(
            0, paint.GetGeometries(chart.CoreCanvas).Count(),
            "a platform without a band geometry draws no bands, consistently");
    }

    // ------------------------------------------------ the zebra computation (internal)

    [TestMethod]
    public void EvenSteps_EveryOtherCellIsBanded()
    {
        // The axis enumerates one step past each end (-10 and 110 here); cells at even step
        // quotients are banded: [0,10), [20,30), ... and [-10,0) is odd so it is skipped.
        var separators = Enumerable.Range(-1, 13).Select(i => i * 10d).ToArray();
        var bands = new Axis().ComputeAlternatingBands(0, 100, separators)!;

        CollectionAssert.AreEqual(
            new[] { 0d, 20, 40, 60, 80, 100 },
            bands.Select(b => b.Start).ToArray(),
            "every other cell, anchored to the step quotient");
        Assert.IsTrue(bands.All(b => Math.Abs(b.End - b.Start - 10) < 1e-9), "each band spans one step");
    }

    [TestMethod]
    public void Pan_DoesNotSwapColors()
    {
        var axis = new Axis();

        // Two windows of the same zoom, one step apart — the cells they share must keep the
        // same parity (a list-index anchor would flip every color on this pan).
        var before = axis.ComputeAlternatingBands(0, 100, [.. Enumerable.Range(-1, 13).Select(i => i * 10d)])!;
        var after = axis.ComputeAlternatingBands(10, 110, [.. Enumerable.Range(0, 13).Select(i => i * 10d)])!;

        var sharedBefore = before.Select(b => b.Start).Where(s => s is >= 10 and <= 90);
        var sharedAfter = after.Select(b => b.Start).Where(s => s is >= 10 and <= 90);

        CollectionAssert.AreEqual(
            sharedBefore.ToArray(), sharedAfter.ToArray(),
            "cells visible in both windows must keep their band parity across the pan");
    }

    [TestMethod]
    public void EdgeCells_AreIncluded()
    {
        // Grouped-style separators are exact boundaries inside the range; the partial cells
        // before the first and after the last separator must still take part in the zebra.
        var bands = new Axis().ComputeAlternatingBands(5, 95, [10d, 30, 50, 70, 90])!;

        var min = bands.Min(b => b.Start);
        var max = bands.Max(b => b.End);

        Assert.IsTrue(min <= 10, "the partial cell before the first separator is considered");
        Assert.IsTrue(max >= 90, "the partial cell after the last separator is considered");
    }

    [TestMethod]
    public void GroupedAxis_BandsFollowTheGroupedSeparators_AndSurviveAPan()
    {
        var axis = new DateTimeAxis(TimeSpan.FromDays(1), d => d.ToString("d")) { GroupTimeUnits = true };

        static double[] GroupedSeparators(DateTime from, DateTime to)
        {
            var ok = DateTimeGrouping.TryGroup(from.Ticks, to.Ticks, out var separators, out _);
            Assert.IsTrue(ok, "the grouped separators must be produced");
            return [.. separators!];
        }

        var fromA = new DateTime(2020, 1, 1);
        var toA = new DateTime(2021, 12, 31);
        var sepsA = GroupedSeparators(fromA, toA);
        var bandsA = axis.ComputeAlternatingBands(fromA.Ticks, toA.Ticks, sepsA)!;

        // Pan three months forward at the same zoom; months present in both windows must keep
        // their parity (the DateTimeAxis anchors parity to the grouped cell ordinal).
        var fromB = new DateTime(2020, 4, 1);
        var toB = new DateTime(2022, 3, 31);
        var sepsB = GroupedSeparators(fromB, toB);
        var bandsB = axis.ComputeAlternatingBands(fromB.Ticks, toB.Ticks, sepsB)!;

        var sharedA = bandsA.Select(b => b.Start).Where(s => s >= fromB.Ticks && s < toA.Ticks).ToArray();
        var sharedB = bandsB.Select(b => b.Start).Where(s => s >= fromB.Ticks && s < toA.Ticks).ToArray();

        Assert.IsTrue(sharedA.Length > 0, "the windows overlap, so they share banded cells");
        CollectionAssert.AreEqual(sharedA, sharedB, "shared cells must keep their parity across the pan");
    }
}
